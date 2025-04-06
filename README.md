
# Simple Pizza Winkel
## What you need

- [.NET9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- Docker client
- Make

## Running Locally

1. Build container images using `make build`
2. Launch backend services 
	1. Start core infrastructure: `docker-compose up -d`
	2. Wait for all containers to initialize
	3. Start application services: `docker compose -f docker-compose-services.yml up -d` 3. 
3. Launch frontend 
	1. Run: `make start-frontend` 
4. Access the application 
	1. Register a new user to begin using the system
	2. Login: http://localhost:3000/login (Default admin credentials: admin@simplepizzawinkel.com / ppp)

## Starting an individual service

All the individual microservices can run independently, and all follow the same structure inside their respective folder under [src](./src/):

5. Start up required infrastructure: `docker-compose up -d`
6. Start up the API component and Dapr sidecar, you'll need two separate terminal windows:
    - `make local-api`
    - `make dapr-api-sidecar`
7. If the specific microservice has a worker component for handling events start them as well, you'll need two more terminal windows:
    - `make local-worker`
    - `make dapr-worker-sidecar`
8. You can switch out either `make local-api` or `make local-worker` with starting the application inside your IDE in debug mode

This will run the individual microservice locally.
