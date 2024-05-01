# BookShop Project contains:
## BookShop.API - Web API and BookShop.Application - Web Application
Before starting BookShop.Application first needs to be ensure that BookShop.API is running, because BookShop.Application depends on BookShop.API

## Web API with ASP.NET Core.
HTTP controller-based API for e-comerce Book Store website that uses Mongo Atlas shared Replica Set database for retriving products(books information) and local SQL (localdb)\MSSQLLocalDB Server for user accounts and orders.<br/>
Requiers internet connection and SQL Server (localdb)\MSSQLLocalDB with out password.

  ### About API:
  Async methods in API, providing the ability to handle several concurrent HTTP requests. They are not blocking the main thread while waiting for the database response. <br>
  This API consumes and produces data in Json format, because this format is simple and lightweighted.
 #### Current API supports 3 API-Versions.
  First API-Version is for Admin access only.<br>
  Second API-Version for User's  access. <br/>
  Third API-Version for not Signed in users, for guests.

### About Mongo Atlas Database:
Mongo Atlas Shared Replica set contains all data about books library.
### About Local SQL:
SQL used for Authentication and Authorization for API and to store Orders details for each user. <br>

### About API Security:
All session tokens, api version, api-key and user token sends as Headers.
To use API-Version:2 and API-Version:3, don't need to provide JWT token. But this versions only supports some GET methods from StockController.cs and POST methods from AuthenticationConroller.cs. The list of all methods and supporting API-Version for each method could be found at the end of this page. <br/>
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
   <tr>
      <td>DELETE/authorization/account/delete/anotheruser</td>
      <td>Deletes an account</td>
      <td>ApiVersion-BooksStore: 1</td>
      <td>Required to be signed in as admin and to provide correct data for account that needs to be deleted.</td>
    </tr>
  <tr><td colspan="4">OrderV Controller</td><tr>
  <tr>
    <td>POST/order</td>
    <td>Creates new order if user signed in and doesn't have currently unsubmitted orders.</td>
    <td>ApiVersion-BooksStore: 1 <br> ApiVersion-BooksStore: 2 </td>
    <td>Required to be signed in and have unsubmitted orders</td>
  </tr>
  <tr>
    <td>PUT/order/products/add</td>
    <td>Adds products to existing order</td>
    <td>ApiVersion-BooksStore: 1 <br> ApiVersion-BooksStore: 2 </td>
    <td>Required to provide valid product/products id, that currently is available at database and valid order id.<br/>
      Adds products if current order exists in database for currently signed in user and not submitted yet.</td>
  </tr>
  <tr>
    <td>PUT/order/products/delete</td>
    <td>Deletes products from existing order for currently signed in user</td>
    <td>ApiVersion-BooksStore: 1 <br> ApiVersion-BooksStore: 2 </td>
    <td>Requered to provide order id and product id<br/>
      Deletes requested product/products from order, if current order exists in database for currently signed in user and not submitted yet.</td>
  </tr>
  <tr>
    <td>PUT/order/submit</td>
    <td>Submits order</td>
    <td>ApiVersion-BooksStore: 1 <br> ApiVersion-BooksStore: 2 </td>
    <td>Required to be signed in and provide valid (for currently signed in user) and valid order id, currently unsubmitted.</td>
  </tr>
   <tr>
    <td>PUT/order/submit</td>
    <td>Submits order</td>
    <td>ApiVersion-BooksStore: 1 <br> ApiVersion-BooksStore: 2 </td>
    <td>Required to be signed in and provide valid (for currently signed in user) and valid order id.<br/>
      Order shoud be unsubmitted and have at least one valid product in it.</td>
  </tr>
   <tr>
    <td>PUT/account/order/unsubmit</td>
    <td>Unsubmits order</td>
    <td>ApiVersion-BooksStore: 1</td>
    <td>Required to be signed in as admin and provide existing order id. Available access to all orders in database(for any user)<br/>
      Order shoud not be submitted.</td>
  </tr>
   <tr>
    <td>PUT/order/details</td>
    <td>Gets requested order details</td>
    <td>ApiVersion-BooksStore: 1<br>ApiVersion-BooksStore: 2</td>
    <td>Required to be signed in and provide order id. If signed in as admin can access to any order in database, if signed in as user can access only own orders.</td>
  </tr>
  <tr>
    <td>PUT/order/all</td>
    <td>Gets all orders for currently signed in user.</td>
    <td>ApiVersion-BooksStore: 1<br>ApiVersion-BooksStore: 2</td>
    <td>Required to be signed in. Gets only own orders for signed in user.</td>
  </tr>
   <tr>
    <td>DELETE/order/delete</td>
    <td>Deletes requested order.</td>
    <td>ApiVersion-BooksStore: 1</td>
    <td>Required to be signed in as admin. Deletes any existing order.</td>
  </tr>
  <tr><td colspan="4">StockV Controller</td><tr>
  <tr>
    <td>GET/books/all</td>
    <td>Retrieves all data from database.</td>
    <td>ApiVersion-BooksStore: 1 <br> ApiVersion-BooksStore: 2 </td>
    <td>For ApiVersion-BooksStore : 2 retrieves only were parameter “isAvailable” : true</td>
  </tr>
   <tr>
    <td> <a name="name_of_target" id="name_of_target">GET/books/page</td>
    <td>Returns a list of requested products for requested page</td>
    <td>ApiVersion-BooksStore: 1 <br> ApiVersion-BooksStore: 2 </td>
    <td>Returns list of requested quantity of products for requested page.<br/>
      The list is sorted in requested order, by requested parameter(by Title or by Author or by Price).<br/>
      If not specified sorting order && parameter for order, then its uses default parameters: descending order by Price.<br/>
      If requested page < 1, then returns page 1, if requested page is larger then total pages, then returns last page.<br/>
      The requested quantity of products per page should be in range 5 to 30. If requested quantity < 5 returns 5 products, if > 30 returns 30.<br/>
      For ApiVersion-BooksStore : 2 retrieves only were parameter “isAvailable” : true
    </td>
  </tr>
  <tr>
    <td>GET/book/id</td>
    <td>Returns product with requested id.</td>
    <td>ApiVersion-BooksStore: 1<br/>ApiVersion-BooksStore: 2<br/>ApiVersion-BooksStore: 3</td>
    <td>For ApiVersion-BooksStore : 2 and ApiVersion-BooksStore: 3, retrieves only were parameter “isAvailable” : true</td>
  </tr>
  <tr>
    <td>GET/books/available</td>
    <td>Returns list of products where parameter isAvailable equals requested true or false.</td>
    <td>ApiVersion-BooksStore: 1</td>
    <td></td>
  </tr>
  <tr>
    <td>GET/books/available/page</td>
    <td>Returns list of products where parameter isAvailable equals requested true or false, at requested page with requested quantity.</td>
    <td>ApiVersion-BooksStore: 1</td>
    <td>Deviding content into pages as in: <a href="name_of_target">GET/books/page</a></td>
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

