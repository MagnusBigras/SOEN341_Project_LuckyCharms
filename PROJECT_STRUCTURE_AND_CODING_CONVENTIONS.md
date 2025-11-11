# Lucky Charm Event Tracker - Project Structure and Coding Conventions

## Package Organization

The project is organized into the following folders/packages for clarity and separation of concerns. These are examples, and other related files or folders are grouped together in a similar way to keep the structure organized and easy to maintain.

**Controllers:** Contains all controller classes handling HTTP requests (e.g., `EventOrganizerController`, `TicketController`).  
**Enums:** Contains enumerations used throughout the project (e.g., `AccountTypes`, `TicketTypes`).  
**Globals:** Contains global variables and session management classes.  
**Helpers:** Contains utility/helper classes, such as `QRCodeGeneratorHelper` and `CSVCreationHelper`.  
**Migrations:** Contains Entity Framework database migration files.  
**Models:** Contains all database entity classes and the `WebAppDBContext`.  
**Pages:** Contains Razor pages used for the frontend views.  
**Services:** Contains service classes for business logic, if applicable.  
**wwwroot:** Contains static files like CSS, JavaScript, images, and libraries.  

Files with similar functionality are grouped together to improve maintainability and readability.

---

## Code Quality

The code looks good overall. Clean structure, well-separated layers, and easy to navigate. Nothing that screams “code smell.” There might be a few small differences in formatting here and there, but nothing serious. Everything runs smoothly and looks organized.

---


## Coding Style and Conventions

- **Indentation:** 
  - Nested blocks are generally indented by one tab or 4 spaces.
  - For long statements spanning multiple lines, additional indentation is used for clarity.
  - Indentation may vary slightly between files due to project evolution.

- **Braces `{ }`:** 
  - Most braces are placed on the same line as the statement.
  - Some files may have braces on the next line. Both styles are accepted in this project.

- **Spacing:** 
  - One blank line is used between functions or major code blocks to improve readability.

- **Naming Conventions:** 
  - **Classes and Enums:** PascalCase (e.g., `EventOrganizer`, `TicketTypes`)
  - **Methods:** PascalCase (e.g., `CreateEventOrganizer()`, `GetTickets()`)
  - **Variables and Parameters:** camelCase (e.g., `updatedEvent`, `ticketId`)
  - **Constants:** UPPER_SNAKE_CASE

- **Consistency:** 
  - While some files may have minor deviations, we aim to follow the above style throughout the project for readability and maintainability.
