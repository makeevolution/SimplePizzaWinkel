
# Plant Based Pizza

This folder contains all the sample application for running the PlantBasedPizza application. 

## Prerequisites

- [.NET9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- Docker client
- Make
    - [For Windows](https://gnuwin32.sourceforge.net/packages/make.htm)
    - [For Mac](https://formulae.brew.sh/formula/make)
    - [Linux](https://askubuntu.com/questions/161104/how-do-i-install-make)


## Running Locally

There are several steps to running the application locally:

1. Build the container images for all services: `make build` or `make-build-arm` depending on your system CPU architecture
2. Start the backend service containers and required infrastructure: `docker-compose up -d`, wait for all containers to start and then `docker compose -f docker-compose-services.yml up -d`
3. Start the frontend: `make start-frontend`
4. Once up and running you can go and register a new user to start interacting with the system
    - If you are trying to login to the [admin interface](http://localhost:3000/admin/login) a default user is created with credentials `admin@plantbasedpizza.com`:`AdminAccount!23`

## Starting an individual service

All the individual microservices can run independently, and all follow the same structure inside their respective folder under [src](./src/):

1. Start up required infrastructure: `docker-compose up -d`
2. Start up the API component and Dapr sidecar, you'll need two separate terminal windows:
    - `make local-api`
    - `make dapr-api-sidecar`
3. If the specific microservice has a worker component for handling events start them as well, you'll need two more terminal windows:
    - `make local-worker`
    - `make dapr-worker-sidecar`
4. You can switch out either `make local-api` or `make local-worker` with starting the application inside your IDE in debug mode

This will run the individual microservice locally.

# overview
![[Drawing 2024-12-23 09.28.39.excalidraw.png]]

# Notes on online course videos

## storage-first API:

![[Drawing 2024-12-23 10.14.03.excalidraw | 800x400]]
## Frontend updates in an async world 

- So the backend updates are done through outbox pattern.
- How about the frontend? How do we update the frontend periodically without user having to refresh the page?
- Polling vs. WebSockets
	- Polling: Frontend periodically calls a backend API to update its contents
	- Websockets: Frontend connects to a websocket to backend where the backend can send a notification to the frontend after an event is published, and the frontend can display that notification (min 3:35 and 8:40)

## Thin Events and Smarter callbacks

- Basically in this vid, it shows how events should be thin, and more details can be obtained by e.g. making a callback to the service publishing the event through some other channel that has strong type definitions for their interfaces e.g. GRPC
- In min 1:50, we see the details of the orderConfirmedEvent, and how that event only contains the eventId (i.e. thin) 
- The consumer of this event i.e. the kitchen service min 3:14, will need more info on the order to get the correct recipe (for example)! 
- Recipes are highly complex (contain a lot of fields of strings, enums etc.) and sending it as an event data along with the order id can cause breaking things
- Minute 4:25 and 5:38 shows how we can get the details of the order by making a callback through GRPC to the order service using the event ID
- This ensures details are transmitted to the outside world through a mechanism that is strongly typed, and events (which inherently don't support such typing) can remain lightweight and dont introduce potential breaking changes

## Idempotency

- Same message published multiple times must be handled only once by the consumer
	- Important e.g. to prevent the same event being handled twice e.g. prevent double charging of customer funds!
- To achieve this:
	- Publisher needs to have an event ID in its header min 2:42
	- When consumer receives the event, grab the event ID and check if it has been processed before (min 4:41) through a cache mechanism (min 4:58 to 5:10) e.g. the IDistributedCache of ASP.NET Core
	- If not yet been processed, process it, then register it in cache (min 6:35) with some ttl min 6:47
		- Important to process it first, especially doing this in a bare except catch all (min 6:39!)
		- Length of ttl is depending on business logic; evaluate your own business case

## Versioning events and evolvability
- If we want to add e.g. new fields to our events, how should we do it (min 1:10)?
- Best way is to:
	- Add V1 to all our current events (version 1)
	- Make a new version V2 with the new field (min 2:28)
	- Create a publisher for this V2 event min (min 3:10)
	- In the method that published V1, also add the V2 event publisher to publish V2 events (min 4:00)
	- The outbox will pickup these 2 versions of the same event and publish them
- Set a deprecation date for your v1 event (min 5:00)!
	- Once the date is hit then can easily remove this publisher of V1 events from the cache 
- Evolvability: Your RabbitMQ routing keys should have event version at the end e.g. <eventname>.<eventversion> so subscribers can listen to different versions of that event min 6:10!

# My own notes

## The flow of the program in general
  See Program.cs of each module's API section of Microservice.
  The below uses the `submit` endpoint of `orders` as an example and also notes on it.

  - `Program.cs` of each microservice's application is the entrypoint to the application
	  - Some `builder.Services` use extension methods feature (e.g. AddOrderManagerInfrastructure) located in `PlantBasedPizza.Orders\application\PlantBasedPizza.OrderManager.Infrastructure\Setup.cs`
	  - All injections to interfaces are done in this class; please study it line by line
		  - e.g. injecting `OrderEventPublisher` to every call to `IOrderEventPublisher` arguments
  - API endpoints are defined in `OrderEndpoints.cs`.
	  - Notice that each endpoint takes in a Handler 
	  - [FromServices] attribute on the argument of the API, means the argument is injected by the dependency injection framework instead of provided by the client
  - Notice that many of the API endpoints has the line `handler.Handle` in its body
	  - All handlers are located under `PlantBasedPizza.Orders\application\PlantBasedPizza.OrderManager.Core\`
	  - Notice that Handler takes in a repository in its constructor, implemented under `PlantBasedPizza.Orders\application\PlantBasedPizza.OrderManager.Infrastructure\OrderRepository.cs`
  - Notice in the body of many of the repository (mainly those that does updates/writes), they do two things:
	  - saves/reads to/from relevant table in DB
	  - saves events to the outbox
  - Background worker will pick up outbox events and publish it 
	  - Background worker located in `PlantBasedPizza.Orders\application\PlantBasedPizza.Orders.Worker\Program.cs`
		  - It is NOT configured by the `app.MapPost`! See below for what these are for!
		  - The background work is configured by `AddHostedService<OutboxWorker>()`!
	  - The background worker under `ExecuteAsync` checks `EventType` of each entry inside the outbox
	  - Then it will publish using the appropriate handler ( `_eventPublisher.PublishOrderCompletedEventV1(orderCompletedIntegrationEvt)` as example) 
  - Consumer picks up msg from messaging bus, and processes it
	  - You need to understand first how Dapper works, explained below.
	  - The endpoints are configured under worker's `Program.cs` `app.MapPost()`
	  - We use Dapper pub-sub in this project to publish/listen to the messaging bus (in this case `redis`)
	  - Under e.g. `PlantBasedPizza.Orders\components` you see the dapper `configuration` for the application; where is this actually used?
	  - Look at [[#Starting an individual service]] section; there you see the fact that we start up the required dependencies using `docker-compose.yml`, and then start the application (be it only consuming or also producing) using the `make` command.
        ```plaintext
        Producer
           |
           v
        Message Bus (e.g., Redis, Kafka)
           |
           v
        Dapr Sidecar (Monitors Message Bus)
           |
           v
        HTTP Endpoint in Application (Processes Message)
		
      In the `make` command of the sidecar there is `--resources-path`; it is where this `configuration` is used.

## OpenTelemetry
