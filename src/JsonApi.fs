namespace MF.JsonApi.Example

//
// Errors
//

type JsonApiErrorDto = {
    Status: string
    Title: string
    Detail: string
}

[<RequireQualifiedAccess>]
module JsonApiErrorDto =
    let notFound message =
        {
            Status = "404"
            Title = "Resource Not Found"
            Detail = message
        }

type JsonApiErrorResponseData = {
    Errors: JsonApiErrorDto list
}

[<RequireQualifiedAccess>]
module JsonApiErrorResponseData =
    let ofErrors errors = {
        Errors = errors
    }

    let ofError error = ofErrors [ error ]

//
// Json Api Context
//

open Microsoft.AspNetCore.Http

type JsonApiContext = {
    HttpContext: HttpContext
}

[<RequireQualifiedAccess>]
module JsonApiContext =

    let getCtx (ctx: HttpContext) = async {
        return Ok {
            HttpContext = ctx
        }
    }
