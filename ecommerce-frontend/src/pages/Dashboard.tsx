import React from "react";
import { useAuth } from "../contexts/AuthContext";
import { useNavigate } from "react-router-dom";
import {
  Box,
  Paper,
  Typography,
  Button,
  Grid,
  Card,
  CardContent,
  Avatar,
  Chip,
} from "@mui/material";
import {
  Person as PersonIcon,
  Email as EmailIcon,
  CalendarToday as CalendarIcon,
} from "@mui/icons-material";

const Dashboard: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  if (!user) return null;

  return (
    <Box sx={{ minHeight: "100vh", backgroundColor: "#f5f5f5", py: 4 }}>
      <Box sx={{ maxWidth: 1200, mx: "auto", px: 3 }}>
        {/* Header */}
        <Paper elevation={2} sx={{ p: 3, mb: 3 }}>
          <Box
            display="flex"
            justifyContent="space-between"
            alignItems="center"
          >
            <Typography variant="h4" component="h1">
              Welcome, {user.firstName}!
            </Typography>
            <Button variant="outlined" color="error" onClick={handleLogout}>
              Logout
            </Button>
          </Box>
        </Paper>

        {/* User Profile Card */}
        <Grid container spacing={3}>
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Box display="flex" alignItems="center" mb={3}>
                  <Avatar
                    sx={{
                      width: 80,
                      height: 80,
                      bgcolor: "primary.main",
                      mr: 2,
                      fontSize: "2rem",
                    }}
                  >
                    {user.firstName[0]}
                    {user.lastName[0]}
                  </Avatar>
                  <Box>
                    <Typography variant="h5">
                      {user.firstName} {user.lastName}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      User Profile
                    </Typography>
                  </Box>
                </Box>

                <Box>
                  <Box display="flex" alignItems="center" mb={2}>
                    <EmailIcon sx={{ mr: 1, color: "text.secondary" }} />
                    <Typography variant="body1">{user.email}</Typography>
                  </Box>

                  <Box display="flex" alignItems="center" mb={2}>
                    <PersonIcon sx={{ mr: 1, color: "text.secondary" }} />
                    <Box>
                      {user.roles.map((role) => (
                        <Chip
                          key={role}
                          label={role}
                          size="small"
                          sx={{ mr: 0.5 }}
                        />
                      ))}
                    </Box>
                  </Box>

                  <Box display="flex" alignItems="center" mb={2}>
                    <CalendarIcon sx={{ mr: 1, color: "text.secondary" }} />
                    <Typography variant="body2" color="text.secondary">
                      Joined: {new Date(user.createdAt).toLocaleDateString()}
                    </Typography>
                  </Box>

                  {user.lastLoginAt && (
                    <Box display="flex" alignItems="center">
                      <CalendarIcon sx={{ mr: 1, color: "text.secondary" }} />
                      <Typography variant="body2" color="text.secondary">
                        Last Login:{" "}
                        {new Date(user.lastLoginAt).toLocaleString()}
                      </Typography>
                    </Box>
                  )}
                </Box>
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Account Status
                </Typography>
                <Box mt={2}>
                  <Typography
                    variant="body2"
                    color="text.secondary"
                    gutterBottom
                  >
                    Status
                  </Typography>
                  <Chip
                    label={user.isActive ? "Active" : "Inactive"}
                    color={user.isActive ? "success" : "error"}
                    size="small"
                  />
                </Box>

                <Box mt={3}>
                  <Typography
                    variant="body2"
                    color="text.secondary"
                    gutterBottom
                  >
                    User ID
                  </Typography>
                  <Typography variant="body2" sx={{ fontFamily: "monospace" }}>
                    {user.id}
                  </Typography>
                </Box>
              </CardContent>
            </Card>
          </Grid>
        </Grid>

        {/* Success Message */}
        <Paper elevation={2} sx={{ p: 3, mt: 3, bgcolor: "success.light" }}>
          <Typography variant="h6" gutterBottom>
            🎉 Congratulations!
          </Typography>
          <Typography variant="body1">
            Your frontend is successfully connected to the backend API running
            on AWS RDS! You've built a complete authentication system with:
          </Typography>
          <Box component="ul" sx={{ mt: 2 }}>
            <li>React Frontend with TypeScript</li>
            <li>.NET 8 Backend API</li>
            <li>AWS RDS SQL Server Database</li>
            <li>JWT Authentication</li>
            <li>Material-UI Components</li>
          </Box>
        </Paper>
      </Box>
    </Box>
  );
};

export default Dashboard;
