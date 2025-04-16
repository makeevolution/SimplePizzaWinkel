// src/App.js
import React, { useEffect, useState } from "react";
import {
  Box,
  Button,
  IconButton,
  Card,
  CardContent,
  Container,
  CssBaseline,
  Grid,
  Typography,
  Divider,
  ListItem,
  AspectRatio,
  Drawer,
  List,
  CardOverflow,
  Skeleton,
} from "@mui/joy";
import recipeService from "../services/recipeService";
import { ordersApi } from "../axiosConfig";
import { Add, ShoppingCart } from "@mui/icons-material";
import Snackbar from "@mui/joy/Snackbar";
import { NotificationHub } from "./SignalR";

function Home() {
  const [menuItems, setMenuItems] = useState({});
  const [order, setOrder] = useState({ items: [] });
  const [orderSummaryOpen, setOrderSummaryOpen] = React.useState(false);
  const [snackbarOpen, setSnackbarOpen] = React.useState(false);
  const [snackbarContents, setSnackbarContents] = React.useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [isVisible, setIsVisible] = useState(false);
  const [isScrollVisible, setIsScrollVisible] = useState(false);

  useEffect(() => {
    // Trigger the animation after the component mounts
    setIsVisible(true);
  }, []);

  useEffect(() => {
    const handleScroll = () => {
      const scrollPosition = window.scrollY;
      const triggerPosition = 200; // Adjust this value as needed

      if (scrollPosition > triggerPosition) {
        setIsScrollVisible(true);
      } else {
        setIsScrollVisible(false);
      }
    };

    window.addEventListener("scroll", handleScroll);

    return () => {
      window.removeEventListener("scroll", handleScroll);
    };
  }, []);
  
  useEffect(() => {
    const handleScroll = () => {
      const scrollPosition = window.scrollY;
      const triggerPosition = 200; // Adjust this value as needed

      if (scrollPosition > triggerPosition) {
        setIsScrollVisible(true);
      } else {
        setIsScrollVisible(false);
      }
    };

    window.addEventListener("scroll", handleScroll);

    return () => {
      window.removeEventListener("scroll", handleScroll);
    };
  }, []); 
  
  const hub = new NotificationHub((message) => {
    console.log("Handling received message:", message);
    setSnackbarContents(message);
    setSnackbarOpen(true);
  });

  const handleSnackbarClose = () => {
    setSnackbarContents("");
    setSnackbarOpen(false);
  };

  const toggleDrawer = (inOpen) => (event) => {
    if (
      event.type === "keydown" &&
      (event.key === "Tab" || event.key === "Shift")
    ) {
      return;
    }

    setOrderSummaryOpen(inOpen);
  };

  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await recipeService.listRecipes();
        const data = response;

        // Group menu items by category
        const groupedData = data.reduce((acc, item) => {
          const category = item.category;
          if (!acc[category]) {
            acc[category] = [];
          }
          acc[category].push(item);
          return acc;
        }, {});

        setMenuItems(groupedData);
        setIsLoading(false);
      } catch (error) {
        console.error("Error fetching the menu data:", error);
      }
    };

    fetchData();
  }, []);

  function getImage(category) {
    console.log(category);
    switch (category) {
      case "Pizza":
      case "0":
        return "/pizza-default.jpg";
      case "Sides":
      case "1":
        return "/fries.jpg";
      case "Drinks":
      case "2":
        return "/can-default.jpg";
      default:
        return "Other";
    }
  }

  async function addToOrder(item) {
    try {
      let orderNumber = order.orderNumber;

      if (orderNumber === undefined) {
        let startOrder = await ordersApi.post("/pickup", {
          customerIdentifier: "",
        });
        orderNumber = startOrder.data.orderNumber;
      }

      let addItemBody = {
        OrderIdentifier: orderNumber,
        RecipeIdentifier: item.recipeIdentifier.toString(),
        Quantity: 1,
      };
      // Make request to add item to order
      let addItemResponse = await ordersApi.post(
        `/${orderNumber}/items`,
        addItemBody
      );

      setSnackbarContents(
        `${item.recipeIdentifier.toString()} added to order!`
      );
      setSnackbarOpen(true);

      setOrder(addItemResponse.data);
    } catch (error) {
      if (error.response.status === 401) {
        setSnackbarContents(`Login is required; please login first or create an account!`);
        setSnackbarOpen(true);
      }
    }
  }

  async function submitOrder() {
    let orderNumber = order.orderNumber;

    if (orderNumber === undefined) {
      return;
    }

    // Make request to add item to order
    await ordersApi.post(`/${orderNumber}/submit`, {
      OrderIdentifier: orderNumber,
      CustomerIdentifier: "",
    });

    setSnackbarContents("Order submitted!");
    setSnackbarOpen(true);

    setOrder({ items: [] });
  }

  return (
    <div>
      <Box sx={{ flexGrow: 1, marginBottom: 0, paddingBottom: 0 }}>
        <Box component="section" height={"100vh"} width={"100vw"}>
          <Grid container spacing={2} style={{opacity: isVisible ? 1 : 0,
            transform: isVisible ? "translateY(0)" : "translateY(20px)",
            transition: "opacity 1s ease, transform 1s ease"}}>
            <Grid
              xs={12}
              sm={9}
              display="flex"
              justifyContent="center"
              alignItems="center"
              minHeight={"100vh"}
              maxWidth={"100vw"}
            >
              <Typography
                  sx={{fontSize: "5rem", fontWeight: "700", lineHeight: "1", fontFamily: "Roboto"}}
              >
                Simple Pizza Winkel
                <br/>
                <br/>
                Lekker pizza!
                <br/>
                Snel gemaakt! Smaakt goed!
              </Typography>
            </Grid>
          </Grid>
          <Divider />
        </Box>
        <Box sx={{ width: 500 }}>
          <Snackbar
            autoHideDuration={2000}
            variant="solid"
            color="success"
            anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
            open={snackbarOpen}
            onClose={handleSnackbarClose}
          >
            {snackbarContents}
          </Snackbar>
        </Box>
        {/* This defines the drawer that will open on orderSummaryOpen state being true */}
        <Drawer
          open={orderSummaryOpen}
          onClose={toggleDrawer(false)}
          size="md"
          anchor="right"
        >
          <Box
            role="presentation"
            onClick={toggleDrawer(false)}
            onKeyDown={toggleDrawer(false)}
          >
            <List>
              <ListItem>
                <Typography level="h2">Your Order</Typography>
              </ListItem>
              <ListItem>
                <Button
                  color="success"
                  xs={3}
                  style={{ float: "right" }}
                  onClick={() => {
                    submitOrder();
                  }}
                >
                  Submit Order
                </Button>
              </ListItem>
              {order.items.map((item) => (
                <Card
                  key={item.recipeIdentifier}
                  sx={{
                    height: "100%",
                    display: "flex",
                    flexDirection: "column",
                  }}
                >
                  <div>
                    <Typography level="title-lg">
                      {item.recipeIdentifier}
                    </Typography>
                    <Typography level="body-sm">{item.quantity}</Typography>
                  </div>
                </Card>
              ))}
            </List>
          </Box>
        </Drawer>
        <CssBaseline />
        <Container sx={{ pt: 8 }} maxWidth="xl">
          <Grid container spacing={1}>
            <Grid item xs={1}></Grid>
            {isLoading === false ? (
              <Grid item xs={10} style={{opacity: isScrollVisible ? 1 : 0,
                transform: isScrollVisible ? "translateY(0)" : "translateY(20px)",
                transition: "opacity 1s ease, transform 1s ease"}}>
                {Object.keys(menuItems).map((category) => (
                  <div key={category}>
                    <Grid container spacing={4}>
                      {menuItems[category].map((item) => (
                        <Grid
                          item
                          key={item.recipeIdentifier}
                          xs={12}
                          sm={6}
                          md={4}
                        >
                          <Card
                            sx={{
                              maxWidth: "100%",
                              boxShadow: "md",
                              height: "100%",
                            }}
                          >
                            <CardOverflow>
                              <AspectRatio ratio="2">
                                <img
                                  src={getImage(category)}
                                  loading="lazy"
                                  alt=""
                                />
                              </AspectRatio>
                            </CardOverflow>
                            <CardContent
                            sx={{
                              display: "flex",
                              flexDirection: "row",
                              alignItems: "center",
                              justifyContent: "space-between",
                            }}>
                              <Typography level="title-md">
                                {item.name}
                              </Typography>
                              <Typography level="body-sm">
                                {item.price.toFixed(2)}
                              </Typography>
                            </CardContent>
                            <CardOverflow>
                              <Button
                                variant="solid"
                                color="primary"
                                onClick={() => {
                                  addToOrder(item);
                                }}
                                sx={{
                                  position: "relative",
                                  right: 0,
                                  bottom: 0,
                                  zIndex: "100",
                                  paddingLeft: "15px",
                                  paddingTop: "10px",
                                }}
                              >
                                <Add />
                              </Button>
                            </CardOverflow>
                          </Card>
                        </Grid>
                      ))}
                    </Grid>
                    <Divider sx={{ my: 4 }} />
                  </div>
                ))}
              </Grid>
            ) : null}
          </Grid>
        </Container>
      </Box>
      
      {/* This defines the shopping card icon, if there is an item in the order */}
      {order.orderNumber === undefined ? (
        <div></div>
      ) : (
        <div
          style={{
            position: "fixed",
            bottom: "20px",
            right: "20px",
            zIndex: "999",
          }}
        >
          <IconButton
            variant="solid"
            color="primary"
            size="lg"
            onClick={toggleDrawer(true)}
            style={{ height: "100px", width: "100px" }}
          >
            <ShoppingCart />
          </IconButton>
        </div>
      )}
    </div>
  );
}

export default Home;
