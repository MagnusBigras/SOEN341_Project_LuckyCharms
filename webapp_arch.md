# WebApp Architecture

The web application is organized into several key components that work together to provide a seamless experience for users:

- **External Tools:** QR code library, calendar library, chart library for added functionality  
- **Frontend Layer:** Student portal, Organizer portal, Admin portal for different user roles  
- **Backend Server:** Controllers, Models, Session Manager, WebAppDBContext, Program, Startup handling business logic and data flow  
- **SQL Database:** Stores users, events, tickets, and organizer information  
- **Automated API Tests:** Ensure backend APIs work correctly  

Below is a visual representation of how these components interact:

![WebApp Architecture](/Images/architecturediagram.png)
