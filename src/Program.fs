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

let private orFail = function
    | Ok ok -> ok
    | Error e -> failwithf "Error: %A" e

[<RequireQualifiedAccess>]
module CurrentApplication =
    let init () =
        try CurrentApplication.fromEnvironment ()
        with e -> Error <| sprintf "Init error %A" e

    let configureApp (currentApplication: CurrentApplication) (app: IApplicationBuilder) =
        let logger = currentApplication.LoggerFactory.CreateLogger("CurrentApplication")
        logger.LogDebug("Configure application ...")

        app
            .UseGiraffeErrorHandler(fun ex _ ->
                logger.LogError(ex, "Unhandled exception while executing request")
                returnUnknownError
            )
            .UseRouting()
            .UseJsonApiEndpoints<JsonApiContext>()

    let configureServices (currentApplication: CurrentApplication) (baseUrlForJsonApiResources: Uri) (services: IServiceCollection) =
        currentApplication.LoggerFactory.CreateLogger("CurrentApplication").LogDebug("Configure services ...")
        services
            .AddSingleton(currentApplication.LoggerFactory)
            .AddLogging()
            .AddGiraffe()
            .AddRouting()
            .AddJsonApi()
                .GetCtxAsyncRes(
                    JsonApiContext.getCtx
                        currentApplication.Service
                        currentApplication.LoggerFactory
                )
                .BaseUrl(baseUrlForJsonApiResources)
                .RelativeJsonApiRoot("/")
                .Add()

[<EntryPoint>]
let main argv =
    let currentApplication = CurrentApplication.init() |> orFail

    let webApp =
        choose [
            route "/metrics"
                >=> warbler (Metrics.currentState currentApplication.Service >> text)

            setStatusCode 404
                >=> routef "/%s"
                    (JsonApiErrorDto.notFound >> JsonApiErrorResponseData.ofError >> json)
        ]

    let baseUrlForJsonApiResources =
        // we are using https://github.com/fabiolb/fabio for services
        let routerHost = "https://example-router.host"
        Uri (
            sprintf "%s/%s" routerHost (currentApplication.Service |> Service.concat "-")
        )

    let app = application {
        url "http://0.0.0.0:8090/"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip

        app_config (CurrentApplication.configureApp currentApplication)
        service_config (CurrentApplication.configureServices currentApplication baseUrlForJsonApiResources)
    }

    run app
    0
