// src/App.js
import React, {useState, useEffect} from "react";
import { BrowserRouter as Router, Routes, Route, Link } from "react-router-dom";
import {
  Box,
  Typography,
  CssBaseline,
  Button,
  List,
  Divider,
  ListItem,
  Sheet,
  Avatar,
  IconButton,
  extendTheme,
  ModalClose,
  ListItemButton,
  Container,
} from "@mui/joy";
import Home from "./components/Home";
import Login from "./components/Login";
import OrderDetail from "./components/OrderDetail";
import Orders from "./components/Orders";
import { Drawer } from "@mui/joy";
import { datadogRum } from "@datadog/browser-rum";
import Register from "./components/Register";
import { MenuBook, Menu } from "@mui/icons-material";
import KitchenDashboard from "./components/admin/KitchenDashboard";
import CollectionDashboard from "./components/admin/CollectionDashboard";
import authService from "./services/authService";
function App() {
  const [open, setOpen] = React.useState(false);

  const toggleDrawer = (inOpen) => (event) => {
    if (
      event.type === "keydown" &&
      (event.key === "Tab" || event.key === "Shift")
    ) {
      return;
    }

    setOpen(inOpen);
  };

  extendTheme({
    fontFamily: {
      display: "Roboto", // applies to `h1`–`h4`
      body: "Roboto", // applies to `title-*` and `body-*`
    },
  });

  return (
    <Sheet>
      <Router>
        <Box sx={{ flexGrow: 1, bgcolor: "#FAEBD7"}}>
          <CssBaseline />
          <div
            style={{
              backgroundColor: "#FAEBD7",
              position: "fixed",
              top: "20px",
              right: "20px",
              zIndex: "999",
            }}
          >
            <IconButton
              variant="outlined"
              color="neutral"
              onClick={toggleDrawer(true)}
              size="lg"
            >
              <Menu />
            </IconButton>
          </div>
          <div
            style={{
              display: "flex",
              justifyContent: "space-between",
              alignItems: "center",
              padding: "0 20px",
              borderBottom: "solid px whitesmoke",
            }}
          ></div>
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/orders" element={<Orders />} />
            <Route path="/orders/:orderNumber" element={<OrderDetail />} />
            <Route path="/admin/kitchen" element={<KitchenDashboard />} />
            <Route path="/admin/collection" element={<CollectionDashboard />} />
          </Routes>
          <Box sx={{
            bgcolor: "#FAEBD7", 
            pb: 4,
          }} component="footer">
            <Container maxWidth="lg">
              <Typography variant="h6" align="center" gutterBottom>
                Simple Pizza Winkel
              </Typography>
              <Typography
                variant="subtitle1"
                align="center"
                color="text.secondary"
                component="p"
              >
                This is my simple microservices project to experiment and apply my on-the-job learnings.
              </Typography>
              <Box sx={{ mt: 2, display: "flex", justifyContent: "center" }}>
                <Link href="https://www.aldosebastian.com" variant="body2" sx={{ mx: 2 }}>
                  aldosebastian.com
                </Link>
              </Box>
            </Container>
          </Box>
          <Drawer open={open} onClose={toggleDrawer(false)}>
            <Box
              sx={{
                display: "flex",
                alignItems: "center",
                gap: 0.5,
                ml: "auto",
                mt: 1,
                mr: 2,
              }}
            >
              <Typography
                component="label"
                htmlFor="close-icon"
                fontSize="sm"
                fontWeight="lg"
                sx={{ cursor: "pointer" }}
              >
                Close
              </Typography>
              <ModalClose id="close-icon" sx={{ position: "initial" }} />
            </Box>
            <List
              size="lg"
              component="nav"
              sx={{
                flex: "none",
                fontSize: "xl",
                "& > div": { justifyContent: "center" },
              }}
            >
              <ListItemButton onClick={() => (window.location.href = "/")}>
                Home
              </ListItemButton>
              <ListItemButton
                onClick={() => (window.location.href = "/orders")}
              >
                Orders
              </ListItemButton>
              {authService.isAdmin() ? (
                      <ListItemButton
                          onClick={() => (window.location.href = "/admin/kitchen")}
                      >
                        Inspect Kitchen
                      </ListItemButton>
                  ) : null}
              {authService.isAdmin() ? (
                  <ListItemButton
                      onClick={() => (window.location.href = "/admin/collection")}
                  >
                    Inspect Orders Awaiting Collection
                  </ListItemButton>
              ) : null}
              <Divider />
              {localStorage.getItem("token") == null ? (
                <ListItemButton
                  onClick={() => (window.location.href = "/login")}
                >
                  Login
                </ListItemButton>
              ) : (
                <ListItemButton
                  onClick={() => {
                    localStorage.removeItem("token");
                    window.location.href = "/";
                  }}
                >
                  Logout
                </ListItemButton>
              )}
            </List>
          </Drawer>
        </Box>
      </Router>
    </Sheet>
  );
}

export default App;
