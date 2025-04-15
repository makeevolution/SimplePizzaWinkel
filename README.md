
# Simple Pizza Winkel

## Description

This is my personal project, to apply my learnings about microservices that I have seen and worked with in my current/previous software engineering job.

The project is a pizza ordering system that allows users to order pizzas online. It consists of multiple microservices, each responsible for a specific part of the system. 

The documentation is inside [docs](./docs/)

The project is built using .NET 9 and uses Dapr for service-to-service communication.
## What you need to run this project

- [.NET9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- Docker client
- Make

## Running Locally (local build)
0. Go to the root of the project, ensure you have Docker running and Make installed
1. Build container images using `make build`
2. Run the project using `make run-local`
3. Access the application 
	1. Register a new user to begin using the system
	2. Login: http://localhost:3000/login (Default admin credentials: admin@simplepizzawinkel.com / ppp)

## Running Locally (pulled from Docker Hub)
I also have published the images to Docker Hub, so you can run the project without building the images yourself.
Run the project using `make run-prd`, and go to http://localhost:3000/login to access the application. 
The default admin credentials are the same as above.

## Starting an individual service

All the individual microservices can run independently, and all follow the same structure inside their respective folder under [src](./src/):

5. Start up required infrastructure: `docker-compose up -d`
6. Start up the API component and Dapr sidecar, you'll need two separate terminal windows:
    - `make local-api`
    - `make dapr-api-sidecar`
7. If the specific microservice has a worker component for handling events, start them as well, you'll need two more terminal windows:
    - `make local-worker`
    - `make dapr-worker-sidecar`
8. You can switch out either `make local-api` or `make local-worker` with starting the application inside your IDE in debug mode

This will run the individual microservice locally.
