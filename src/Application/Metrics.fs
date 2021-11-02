namespace MF.JsonApi.Example

[<RequireQualifiedAccess>]
module Metrics =
    let currentState (service: Service) _ =
        sprintf "Metrics for %s ..." (service |> Service.concat "-")
