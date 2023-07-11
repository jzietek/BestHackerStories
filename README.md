# BestHackerStories
## Summary
Simple REST API implemented with .NET 7.
Provides a collection of best stories available in the Hacker News API. 
Hacker News API endpoints are configured in the API settings file.
Articles are sorted from highest rated ones in descending order.

## How to run it
1. Make sure you have the .NET 7 SDK installed.
2. Clone this repo. 
3. Build and run it with `dotnet run --project src/BestHackerStories.Api` command in the terminal/command line from the projects main directory. On Windows machine you might need to use `src\BestHackerStories.Api`.
4. REST API will be hosted on local 8080 port.
* It has Swagger page available on http://localhost:8080/swagger/index.html
* Can be called with tools like cURL or Postman with `GET http://localhost:8080/api/beststories`

## Assumptions
* Time for the project is too scarce to have a decent unit tests coverage. But integration tests will spped-up the development feedback cycle.
* Data available under https://hacker-news.firebaseio.com/v0/beststories.json is assumed to be a list of article IDs not ordered by articles scores. All items need to be queried to make a final sorting and picking desired items count. Client should be able to cancel his request.
* Mappings used for DTOs is trivial, hence no AutoMApper / Mapperly / Mappster were used.

## Ideas for extensions
Due to limited time for this project, ideas for future extension are placed here:
* Unit test with xUnit/NUnit + NSubstitute/Moq.
* Paging of results for more control of transfered data quantity.
* Possibility to request reversed ordering.
* Maybe some some JWT authentication could be used. Although the functionality is very simple here.
* API versioning.
* Output caching for better performance.
* Rate limiting for incoming requests.
* TLS support.
* Providing a Docker file.
* HATEOAS