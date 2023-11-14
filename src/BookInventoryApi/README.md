# Book Inventory 
Book Inventory is one of the microservice of Bob's Used Bookstore Serverless.

## How to test

Once deployment is successful, take Api Gateway Url from CDK output and update `api_gateway_url` on below Apis request

**Add Book** - Add new book and return newly created bookid.

Url 
    
    POST https://{{api_gateway_url}}/books

Body

    {
    "name": "2020: The Apocalypse",
    "author": "Li Juan",
    "bookType": "Hardcover",
    "condition": "Like New",
    "genre": "Mystery, Thriller & Suspense",
    "publisher": "Astral Publishing",
    "year": 2023,
    "isbn": "6556784356",
    "summary": "Bobs used book serverless",
    "price": 5,
    "quantity": 10,
    "coverImage": null,
    "coverImageFileName": "/Content/Images/coverimages/apocalypse.png"
    }


**Get Book** - Get Book detail by bookId.

Url 
    
    GET {{api_gateway_url}}/books/177577e4-883f-4f72-a6c8-9e977e05ccfb

