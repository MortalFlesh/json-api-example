namespace MF.JsonApi.Example

open Microsoft.Extensions.Logging

//
// Errors
//

[<RequireQualifiedAccess>]
module Errors =
    let ofErrorWithCode format e =
        let code, title = e |> format

        let error =
            Felicity.Error.create code
            |> Felicity.Error.setTitle title

        [ error ]

    let ofErrorsWithCode format = List.collect (ofErrorWithCode format)

    let ofError format e =
        let error =
            Felicity.Error.create 400
            |> Felicity.Error.setTitle (e |> format)

        [ error ]

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
open Microsoft.Extensions.Logging

type JsonApiContext = {
    CurrentApplication: Service
    HttpContext: HttpContext
    LoggerFactory: ILoggerFactory
    LogJsonApiErrors: Felicity.Error list -> unit
}

[<RequireQualifiedAccess>]
module JsonApiContext =
    let private logJsonApiErrors (loggerFactory: ILoggerFactory) (errors: Felicity.Error list) =
        loggerFactory
            .CreateLogger("JsonApi")
            .LogCritical("Errors: {errors}", errors |> List.map (sprintf "%A"))

    let getCtx
        instance
        (loggerFactory: ILoggerFactory)
        (ctx: HttpContext) = async {
            return Ok {
                CurrentApplication = instance
                HttpContext = ctx
                LoggerFactory = loggerFactory
                LogJsonApiErrors = logJsonApiErrors loggerFactory
            }
        }
