build: build-account build-delivery build-kitchen build-loyalty-points build-orders build-payments build-recipes build-frontend

build-account:
	cd src/PlantBasedPizza.Account;make build
build-delivery:
	cd src/PlantBasedPizza.Delivery;make build
build-kitchen:
	cd src/PlantBasedPizza.Kitchen;make build
build-loyalty-points:
	cd src/PlantBasedPizza.LoyaltyPoints;make build
build-orders:
	cd src/PlantBasedPizza.Orders;make build
build-payments:
	cd src/PlantBasedPizza.Payments;make build
build-recipes:
	cd src/PlantBasedPizza.Recipes;make build
build-frontend:
	cd src/frontend;docker build -t frontend .

build-arm: build-account-arm build-delivery-arm build-kitchen-arm build-loyalty-points-arm build-orders-arm build-payments-arm build-recipes-arm build-frontend

build-account-arm:
	cd src/PlantBasedPizza.Account;make build-arm
build-delivery-arm:
	cd src/PlantBasedPizza.Delivery;make build-arm
build-kitchen-arm:
	cd src/PlantBasedPizza.Kitchen;make build-arm
build-loyalty-points-arm:
	cd src/PlantBasedPizza.LoyaltyPoints;make build-arm
build-orders-arm:
	cd src/PlantBasedPizza.Orders;make build-arm
build-payments-arm:
	cd src/PlantBasedPizza.Payments;make build-arm
build-recipes-arm:
	cd src/PlantBasedPizza.Recipes;make build-arm

run-local:
	docker compose up -d; sleep 10; docker compose -f docker-compose-services.yml up -d; docker run -d   --name mongo-express   --link mongodb:mongo  -e ME_CONFIG_MONGODB_SERVER=mongo   -p 33333:8081 --network complex-producer-consumer-pizza_dapr  mongo-express

stop-local:
	docker compose down; docker compose -f docker-compose-services.yml down; docker rm -f mongo-express