namespace MF.JsonApi.Example

open Microsoft.Extensions.Logging
open MF.JsonApi.Example

type CurrentApplication = {
    Service: Service
    LoggerFactory: ILoggerFactory
}

//
// Current Application module
//

[<RequireQualifiedAccess>]
module CurrentApplication =
    let private loggerFactory =
        LoggerFactory.Create(fun builder ->
            builder
                .SetMinimumLevel(LogLevel.Debug)
                .AddConsole(fun c -> c.LogToStandardErrorThreshold <- LogLevel.Error)
            |> ignore
        )

    let fromEnvironment () =
        // normally this is loaded from environment variables

        Ok {
            Service = { Domain = Domain "Example"; Context = Context "JsonApi" }
            LoggerFactory = loggerFactory
        }
