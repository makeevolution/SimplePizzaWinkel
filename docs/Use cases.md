
#### User register and login
```plantuml
frontend->accountsService: POST /accounts/register with username password
accountsService->accountsService: create user in db with appropriate role, and a JWT token
accountsService->frontend: HTTP 202 created, redirect to login page

```

```plantuml
frontend->accountsService: POST /accounts/login with username password
alt username exists, correct password
	accountsService->accountsService: generate JWT token
	accountsService->frontend: HTTP 202 created with the token
	frontend->frontend: Saves token in Local Storage
else username exists, incorrect password
	accountsService->frontend: HTTP 400 Forbidden
else user does not exist
    accountsService->frontend: HTTP 400 Forbidden
end
```
#### Adding an order
```plantuml
participant frontend
participant ordersService
participant recipeService
participant messagingBus

frontend->recipeService: GET recipe/ to get all available recipes available to add to order
recipeService->frontend: HTTP 200 ok with all recipes
frontend->frontend: Selects a particular recipe


opt no orderId in frontend (in page state)
	frontend->ordersService: POST /order/pickup
	opt user is not authenticated (no/expired JWT)
	    ordersService->frontend: HTTP 401 Forbidden
	    frontend->frontend: Show 'please login before making an order!'
	    note right
		    whole flow stops here
		end note
	end
	par
	  ordersService->ordersService: Creates an order of type pickup
	else
	  ordersService->messagingBus: Publishes event `order.OrderCreated.v1`
	end
	ordersService->frontend: Returns the orderId 
	frontend->frontend: Saves the orderId in page state
end
frontend->ordersService: POST orders/{orderId}/items with the orderId and body containing the recipeID
ordersService->ordersService: Updates the order with orderId, with the new item


```

#### Submitting order and order processing
```plantuml
participant frontend
participant ordersService
participant recipeService
participant kitchenService
participant paymentService
participant messagingBus
participant recipeService

frontend->ordersService: HTTP /orders/submit
par
    ordersService->messagingBus: publish order.OrderSubmitted.v1
    ordersService->messagingBus: publish payments.takePayment.v1
end
messagingBus->paymentService: handle event payments.takePayment.v1, simulate fake payment success or fail through changing code
alt payment successful
	paymentService->messagingBus: publish event payments.paymentSuccessful.v1
	messagingBus->ordersService: handle event payments.paymentSuccessful.v1
	ordersService->messagingBus: publish event orders.orderConfirmed.v1
	messagingBus->kitchenService: handle event orders.orderConfirmed.v1
	kitchenService->recipeService: HTTP GET /recipe/{recipeIdentifier}, get recipe for each item in order
	recipeService->kitchenService: return details of recipes, kitchenService collects all ingredients and recipes
	kitchenService->messagingBus: publish event kitchen.orderPreparing.v1
	messagingBus->ordersService: handle event kitchen.orderPreparing.v1, update order state and notify user through websocket
	kitchenService->messagingBus: publish event kitchen.orderPrepComplete.v1
	messagingBus->ordersService: handle event kitchen.orderPrepComplete.v1, update order state and notify user through websocket
	kitchenService->messagingBus: publish event kitchen.orderBaked.v1
	messagingBus->ordersService: handle event kitchen.orderBaked.v1, update order state and notify user through websocket
	kitchenService->messagingBus: publish event kitchen.qualityChecked.v1
	messagingBus->ordersService: handle event kitchen.qualityChecked.v1, update order state and notify user through websocket, order is awaiting collection
	frontend->ordersService: HTTP POST /order/collected, user collects order
else payment failed
	paymentService->messagingBus: publish event payments.paymentFailed.v1
	messagingBus->ordersService: handle event payments.paymentFailed.v1, update order state and notify user through websocket, order is cancelled
end
```










