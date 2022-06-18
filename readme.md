# Umbraco ContentAPI

## What is it?

It's a package for Umbraco 10 that allows you to explore and fetch data from a standardized API. The aim is to follow the [JSON Hypertext Application Language specification](https://tools.ietf.org/id/draft-kelly-json-hal-01.html#:~:text=The%20JSON%20Hypertext%20Application%20Language,exposed%20as%20series%20of%20links.)

## But why?

Umbraco currently only supports GraphQL, and only on the cloud solutions. Some organizations prefer self-hosting but still want a CMS decoupled from their frontend. 
This package allows for that.

## Okay. But how?

Quite simple. Setup a Umbraco CMS solution and install this package. After creating all of your content types and nodes etc. you can browse the /api/contentapi to 
get a HAL+JSON response with the structure for your data. You can then create a new project with a React, VueJS, Angular or something else and consume the API.
Now your frontend and CMS are decoupled. The API can also be consumed by a mobile app or anything else that can make http requests.

## Okay, but HAL+JSON is old tech. What about GraphQL?

Yeah, HAL+JSON was proposed back in 2012. But the tech is still solid and used by many larger organization - like Facebook. That being said there might be a option
for GraphQL and Swagger somewhere down the road.

## Okay. But I'm new to HAL+JSON. How does it work?

Well it's just like normal JSON, but with some more metadata and links. It is really easy. Come let me show you (TODO: insert link to getting started page) 