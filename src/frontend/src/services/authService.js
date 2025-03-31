import { jwtDecode } from "jwt-decode";

const authService = {
    isAdmin: function () {
        const token = localStorage.getItem("token")
        if (token == null) {  // both checks for null and undefined
            return false
        }
        try {
            const decodedToken = jwtDecode(token)
            return decodedToken.role === "admin";
        }
        catch (error) {
            console.error(error)
            return false;
        }
    },
};

export default authService;
