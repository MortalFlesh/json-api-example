open System
open System.IO

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

open Giraffe
open Saturn
open Felicity

open MF.JsonApi.Example

let configureApp (app: IApplicationBuilder) =
    app
        .UseGiraffeErrorHandler(fun ex _ ->
            eprintfn "Unhandled exception while executing request %A" ex
            returnUnknownError
        )
        .UseRouting()
        .UseJsonApiEndpoints<JsonApiContext>()

let configureServices (baseUrlForJsonApiResources: Uri) (services: IServiceCollection) =
    services
        .AddGiraffe()
        .AddRouting()
        .AddJsonApi()
            // .BaseUrl(baseUrlForJsonApiResources) //! uncomment this line to break the application
            .GetCtxAsyncRes(JsonApiContext.getCtx)
            // .BaseUrl(baseUrlForJsonApiResources) //! uncomment this line to break the application
            .Add()

[<EntryPoint>]
let main argv =
    let webApp =
        choose [
            route "/metrics"
                >=> warbler (fun _ -> text "...Metrics...")

            setStatusCode 404
                >=> routef "/%s"
                    (JsonApiErrorDto.notFound >> JsonApiErrorResponseData.ofError >> json)
        ]

    let baseUrlForJsonApiResources =
        Uri ("https://example-router.host/example-json-api")

    let app = application {
        url "http://0.0.0.0:8090/"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip

        app_config configureApp
        service_config (configureServices baseUrlForJsonApiResources)
    }

    run app
    0
