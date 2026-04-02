# Copilot Instructions

## Project Guidelines
- Use stored procedures via repository helper methods for list/search/sort/pagination logic instead of manual EF sorting/filtering in repositories; controllers should pass parameters from FillParamesFromModel and bind results through BindSearchResult pattern.