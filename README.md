# Web API with ASP.NET Core.

<p>HTTP controller-based API for e-comerce Book Store website that uses Mongo Atlas shared Replica Set database for retriving products(books information) and local SQL (localdb)\MSSQLLocalDB Server for user accounts and orders.</p>

<p>On runing device requiers internet connection and SQL Server (localdb)\MSSQLLocalDB with out password.</p>


  ## About API:
  Async methods in API, providing the ability to handle several concurrent HTTP requests. They are not blocking the main thread while waiting for the database response. <br>
  This API consumes and produces data in Json format, because this format is simple and lightweighted.
 ### Current API supports 2 API-Versions.
   First API-Version is for Admin access only.<br>
  Second API-Version for User's  access.

## About Mongo Atlas Database:
Mongo Atlas Shared Replica set contains all data about books library.
  ### Some pross of NoSQL databases
In NoSQL databases data is stored in a more freeform, without rigid schemas, that makes NoSQL more flexible than SQL.<br>
In case of increase in traffic there is no need to add additional servers for scaling, there is enough hardware for this. NoSQL has high performance as data or traffic increases due to its scalable architecture. NoSQL database can automatically duplicate data across multiple servers, data centers or cloud resources. This benefit helps to minimize latency for users. It also reduces the load on database management.

  ## About Local SQL:
SQL used for Authentication and Authorization for API and to store Orders details for each user. <br>
Previously for it was used Azure SQL, but was decided to change it to local SQL(still looking for better solution), because of too high cost for Azure Services.

## About API Security:
All session tokens, api version, api-key and user token sends as Headers.
To use API-Version:2, don't need to provide JWT token, api-key and user data. But API-Version:2 only supports some GET methods.
Just required to specify API-Version:2.
API-Version:1 supports all methods of API (GET, POST, PUT, DELETE), for access to it requires: api-key and valid JWT token.
To JWT token, required provide api-key and correct admin loging details, after thet API will generate JWT token that valid for 1 hour.
Also there ability to create own administration acccount, for that only required to provide api-key.

<table>
  <thead><td>API:</td><td>Description:</td><td>API Version<br>()</td><td>Additional Notes:</td></thead>
  <tr><td colspan="4">AuthenticationV Controller:</td></tr>
    <tr>
      <td>POST/authorization/loging</td>
      <td>Singing in existing administration account and provides token.<br>
        Default Administration account:<br>UserLogin: admin<br>Password: P@ssw0rd<br>
        Default User account:<br>UserLogin: user<br>Password: P@ssw0rd</td>
      <td>ApiVersion-BooksStore: 1 <br> ApiVersion-BooksStore: 2 </td>
      <td>ApiVersion-BooksStore: 1, supports only to sign under administration accounts</td>
    </tr>
    <tr>
      <td>POST/authorization/register</td>
      <td>Registering new account.<br>
        If specified API-Version is 'ApiVersion-BooksStore: 1' registrated account automatically adds admin role.<br>
        If specified API-Version is 'ApiVersion-BooksStore: 2' registrated account automatically adds only user role.
       </td>
      <td>ApiVersion-BooksStore: 1 <br> ApiVersion-BooksStore: 2 </td>
      <td>Requires unique username, unique email, and password.</td>
    </tr>
    <tr>
      <td>PUT/authorization/password/reset/foranother</td>
      <td>Resets password for any account</td>
      <td>ApiVersion-BooksStore: 1</td>
      <td>Requieres: to be signed in under admin account<br>
      Has to provide: correct registrated email and correct current password for this account and new password</td>
    </tr>
    <tr>
      <td>POST/authorization/password/reset</td>
      <td>Resets password for currently signed in account</td>
      <td>ApiVersion-BooksStore: 1 <br> ApiVersion-BooksStore: 2 </td>
      <td>Requires to be signed in and provide correct current password for this account.</td>
    </tr>
    <tr>
      <td>DELETE/authorization/account/delete</td>
      <td>Deletes currently signed in account</td>
      <td>ApiVersion-BooksStore: 1 <br> ApiVersion-BooksStore: 2 </td>
      <td>Requires to be signed in and provide correct current password for this account.</td>
    </tr>
  
  <tr><td colspan="4">StockV Controller</td><tr>
  <tr>
    <td>GET/books/all</td>
    <td>Retrieves all data from database.</td>
    <td>ApiVersion-BooksStore: 1 <br> ApiVersion-BooksStore: 2 </td>
    <td>For ApiVersion-BooksStore : 2 retrieves only were parameter “isAvailable” : true</td>
  </tr>
  <tr>
    <td>GET/books/page</td>
    <td>Retrieves all data from database, with requested page and requested quantity per page (divides all data from database into pages) and sorts the data in ascending or descending order.</td>
    <td>ApiVersion-BooksStore: 1 <br> ApiVersion-BooksStore: 2 </td>
    <td>For ApiVersion-BooksStore : 2 retrieves only were parameter “isAvailable” : true</td>
  </tr>
  <tr>
    <td>GET/books/count/all</td>
    <td>Returns quantity of all books in database.</td>
    <td>ApiVersion-BooksStore: 1 <br> ApiVersion-BooksStore: 2 </td>
    <td>For ApiVersion-BooksStore : 2 retrieves only were parameter “isAvailable” : true</td>
  </tr>
  <tr>
    <td>POST/book/add</td>
    <td>Add new book/data in database. Requires entering all data, except the Id, Id generated automatically.</td>
    <td>ApiVersion-BooksStore: 1</td>
    <td></td>
  </tr>
  </colgroup>
</table>
Hello
