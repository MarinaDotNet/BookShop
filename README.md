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
| Field in MongoDB: | Data type in MongoDB: | Data type in C#: | Field name in C# class: |
| ----------------- | --------------------- | ---------------- | ----------------------- |
| _id | ObjectId | string | Id |
| annotation | String | string | Annotation |
| author | String | string | Author |
| available | Boolean | bool | IsAvailable |
| book | String | string | Title |
| genre | Array String | string[] | Genres |
| language | String | string | Language |
| link | String | Uri | Link |
| price | Decimal128 | decimal | Price |

### About Local SQL:
SQL used for Authentication and Authorization for API and to store Orders details for each user. <br>
Added custom table dbo.Orders:
| Order | Key | Data Type | Value |
| ----- | --- | --------- | ----- |
| OrderId | PK | nvarchar(450) | not null |
| UserId | FK | nvarchar(450) | not null |
| ProductsId |  | nvarchar(max) | null |
| TotalPrice |  | decimal(18,2) | not null |
| OrderDateTime |  | datetime2(7) | not null |
| SubmittedOrder |  | bit | not null |

### About API Security:
All session tokens, api version, api-key and user token sends as Headers.
To use API-Version:2 and API-Version:3, don't need to provide JWT token. But this versions only supports some GET methods from StockController.cs and POST methods from AuthenticationConroller.cs. The list of all methods and supporting API-Version for each method could be found at the end of this page. <br/>
API-Version:1 supports all methods of API (GET, POST, PUT, DELETE), for access to it requires: api-key and valid JWT token.
To JWT token, required provide api-key and correct admin loging details, after thet API will generate JWT token that valid for 1 hour.
Also there ability to create own administration acccount, for that only required to provide api-key.

## AuthenticationV Controller:
| API: | Description: | API Version: | Additional Notes: |
| ---- | ------------ | ------------ | ----------------- |
| POST/authorization/loging | Singing in existing account and provides token.<br><br>**Default Administration account**:<br>"UserLogin: admin"<br>"Password: P@ssw0rd"<br>**Default User account**:<br>"UserLogin: user"<br>"Password: P@ssw0rd" | ApiVersion-BooksStore: 1<br>ApiVersion-BooksStore: 2 | ApiVersion-BooksStore: 1, supports only to sign under administration accounts |
| POST/authorization/register | Registering new account.<br><br>If specified API-Version is 'ApiVersion-BooksStore: 1' registrated account automatically adds admin role. <br>If specified API-Version is 'ApiVersion-BooksStore: 2' registrated account automatically adds only user role. | ApiVersion-BooksStore: 1<br>ApiVersion-BooksStore: 2 | Requires unique username, unique email, and password. |
| PUT/authorization/password/reset/foranother | Resets password for any account | ApiVersion-BooksStore: 1 | Requieres: to be signed in under admin account.<br>Has to provide: correct registrated email and correct current password for this account and new password |
| POST/authorization/password/reset | Resets password for currently signed in account | ApiVersion-BooksStore: 1<br>ApiVersion-BooksStore: 2 | Requires to be signed in and provide correct current password for this account. |
| DELETE/authorization/account/delete | >Deletes currently signed in account | ApiVersion-BooksStore: 1<br>ApiVersion-BooksStore: 2 | Requires to be signed in and provide correct current password for this account. |
| DELETE/authorization/account/delete/anotheruser | Deletes an account | ApiVersion-BooksStore: 1 | Required to be signed in as admin and to provide correct data for account that needs to be deleted. |

## OrderV Controller:
| API: | Description: | API Version: | Additional Notes: |
| ---- | ------------ | ------------ | ----------------- |
| POST/order | Creates new order if user signed in and doesn't have currently unsubmitted orders. | ApiVersion-BooksStore: 1<br>ApiVersion-BooksStore: 2 | Required to be signed in and have unsubmitted orders |
| PUT/order/products/add | Adds products to existing order | piVersion-BooksStore: 1<br>ApiVersion-BooksStore: 2 | Required to provide valid product/products id, that currently is available at database and valid order id.<br>Adds products if current order exists in database for currently signed in user and not submitted yet. |
| PUT/order/products/delete | Deletes products from existing order for currently signed in user | ApiVersion-BooksStore: 1<br>ApiVersion-BooksStore: 2 | Requered to provide order id and product id<br>Deletes requested product/products from order, if current order exists in database for currently signed in user and not submitted yet. |
| PUT/order/submit | Submits order | ApiVersion-BooksStore: 1<br>ApiVersion-BooksStore: 2 | Required to be signed in and provide valid (for currently signed in user) and valid order id, currently unsubmitted. |
| PUT/order/submit | Submits order | ApiVersion-BooksStore: 1<br>ApiVersion-BooksStore: 2 | Required to be signed in and provide valid (for currently signed in user) and valid order id.<br>Order shoud be unsubmitted and have at least one valid product in it. |
| PUT/account/order/unsubmit | Unsubmits order | ApiVersion-BooksStore: 1 | Required to be signed in as admin and provide existing order id. Available access to all orders in database(for any user)<br>Order shoud not be submitted. |
| PUT/order/details | Gets requested order details | ApiVersion-BooksStore: 1<br>ApiVersion-BooksStore: 2 | Required to be signed in and provide order id. If signed in as admin can access to any order in database, if signed in as user can access only own orders. |
| PUT/order/all | Gets all orders for currently signed in user. | ApiVersion-BooksStore: 1<br>ApiVersion-BooksStore: 2 | Required to be signed in. Gets only own orders for signed in user. |
| DELETE/order/delete | Deletes requested order. | ApiVersion-BooksStore: 1 | Required to be signed in as admin. Deletes any existing order.|

## StockV Controller:
| API: | Description: | API Version: | Additional Notes: |
| ---- | ------------ | ------------ | ----------------- |
| GET/books/all | Retrieves all data from database. | ApiVersion-BooksStore: 1<br>ApiVersion-BooksStore: 2 | For ApiVersion-BooksStore : 2 retrieves only were parameter “isAvailable” : true |
| <span id="GET/books/page" name="GET/books/page">GET/books/page</span> | Returns a list of requested products for requested page | ApiVersion-BooksStore: 1<br>ApiVersion-BooksStore: 2 | Returns list of requested quantity of products for requested page.<br>The list is sorted in requested order, by requested parameter(by Title or by Author or by Price).<br>If not specified sorting order && parameter for order, then its uses default parameters: descending order by Price.<br>If requested page < 1, then returns page 1, if requested page is larger then total pages, then returns last page.<br>The requested quantity of products per page should be in range 5 to 30. If requested quantity < 5 returns 5 products, if > 30 returns 30.<br>For ApiVersion-BooksStore : 2 retrieves only were parameter “isAvailable” : true |
| GET/book/id | Returns product with requested id. | ApiVersion-BooksStore: 1<br>ApiVersion-BooksStore: 2<br>ApiVersion-BooksStore: 3 | For ApiVersion-BooksStore : 2 and ApiVersion-BooksStore: 3, retrieves only were parameter “isAvailable” : true |
| GET/books/available | Returns list of products where parameter isAvailable equals requested true or false. | ApiVersion-BooksStore: 1 |  |
| GET/books/available/page | Returns list of products where parameter isAvailable equals requested true or false, at requested page with requested quantity. | ApiVersion-BooksStore: 1 | Deviding content into pages as in: [GET/books/page](#GET/books/page) method |
| GET/books/genres | Returns a list of all product's genres in database | ApiVersion-BooksStore: 1<br>ApiVersion-BooksStore: 2<br>ApiVersion-BooksStore: 3 | For ApiVersions 2 and 3 returns genres where at least one product isAvailable == true |
| GET/books/genre | Returns a list of products under requested genre, on requested page and requested quantity | ApiVersion-BooksStore: 1<br>ApiVersion-BooksStore: 2 | Devides content into pages, returns requested page simmular to method: [GET/books/page](#GET/books/page).<br>For ApiVersions 2 and 3 returns genres where at least one product isAvailable == true. |
| GET/books/filter/condition/equals | Returns list of ALL products where Title, Author, Language or one of item from array of Genres EQUALS to searchCondition.| ApiVersion-BooksStore: 1<br>ApiVersion-BooksStore: 2 | Devides content into pages, returns requested page simmular to method: [GET/books/page](#GET/books/page).<br>For ApiVersion: 2 returns only products where isAvailable == true. |
| GET/books/filter/condition/contains | Returns list of ALL products where Title, Author, Language or one of item from array of Genres CONTAINS the searchCondition.| ApiVersion-BooksStore: 1<br>ApiVersion-BooksStore: 2 | Devides content into pages, returns requested page simmular to method: [GET/books/page](#GET/books/page).<br>For ApiVersion: 2 returns only products where isAvailable == true. |
| GET/books/filter | Sorts the list of all products by requested parameters. For each specified parameter that contains data. | ApiVersion-BooksStore: 1<br>ApiVersion-BooksStore: 2<br>ApiVersion-BooksStore: 3 | Filters the list by each provided parameter. Exception if IsAvailable was not specified, then its sets as false for ApiVersion 1, and always sets to true for ApiVersions 2 and 3. Devides content into pages, returns requested page simmular to method: [GET/books/page](#GET/books/page). |
| POST/book/add | Adds new book/data in database. Requires entering all data, except the Id, Id generated automatically. | ApiVersion-BooksStore: 1 |  |
| DELETE/book/delete | Deletes product under requested id. | ApiVersion-BooksStore: 1 | Required valid product id |
| PUT//book/update | Updates product under requested id with provided data. | ApiVersion-BooksStore: 1 | Required valid product id.<br>If some parameters was not specified, then it keeps old data for this parameters. |
| GET/books/count/all | Returns quantity of all products in database. | ApiVersion-BooksStore: 1<br>ApiVersion-BooksStore: 2 | For ApiVersion-BooksStore : 2 retrieves only were parameter “isAvailable” : true. |
| GET/books/count/available | Returns quantity of products where products as requested isAvailable == true or false | ApiVersion-BooksStore: 1 | Requered to specify isAvailable == true or false |
| GET/books/count/ingenre | Returns quantity of products under specified genre | ApiVersion-BooksStore: 1<br>ApiVersion-BooksStore: 2 | Reguered to specify valid genre.<br>For ApiVersion: 2 counts only products with parameter isAvailable == true. Could return 0 if there nor products under specified conditions |



