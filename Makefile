build: build-account build-delivery build-kitchen build-orders build-payments build-recipes build-frontend
push: push-account push-delivery push-kitchen push-orders push-payments push-recipes push-frontend

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
	cd src/frontend&&make build

push-account:
	cd src/PlantBasedPizza.Account&&make tag-images&&make push
push-delivery:
	cd src/PlantBasedPizza.Delivery&&make tag-images&&make push
push-kitchen:
	cd src/PlantBasedPizza.Kitchen&&make tag-images&&make push
push-orders:
	cd src/PlantBasedPizza.Orders&&make tag-images&&make push
push-payments:
	cd src/PlantBasedPizza.Payments&&make tag-images&&make push
push-recipes:
	cd src/PlantBasedPizza.Recipes&&make tag-images&&make push
push-frontend:
	cd src/frontend&&make tag-images&&make push

run-local:
	docker compose -f deployment/docker-compose-infra.yml up -d&& sleep 10&& docker compose -f deployment/docker-compose-services-local.yml up -d&& docker image prune -f

stop-local:
	docker compose -f deployment/docker-compose-infra.yml down&& docker compose -f deployment/docker-compose-services-local.yml down

run-prd:
	cd deployment&& docker compose -f docker-compose-infra.yml up -d&& sleep 10&& docker compose -f docker-compose-services-prd.yml up -d&& cd ..

stop-prd:
	cd deployment&& docker compose -f docker-compose-infra.yml down&& docker compose -f docker-compose-services-prd.yml down && cd ..
