build: build-account build-delivery build-kitchen build-orders build-payments build-recipes build-frontend

build-account:
	cd src/PlantBasedPizza.Account&&make build
build-delivery:
	cd src/PlantBasedPizza.Delivery&&make build
build-kitchen:
	cd src/PlantBasedPizza.Kitchen&&make build
build-orders:
	cd src/PlantBasedPizza.Orders&&make build
build-payments:
	cd src/PlantBasedPizza.Payments&&make build
build-recipes:
	cd src/PlantBasedPizza.Recipes&&make build
build-frontend:
	cd src/frontend&&docker build -t frontend .

run-local:
	docker compose up -d&& sleep 10&& docker compose -f docker-compose-services.yml up -d&& docker image prune -f

stop-local:
	docker compose down&& docker compose -f docker-compose-services.yml down