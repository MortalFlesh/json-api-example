namespace MF.JsonApi.Example.Resource

open MF.JsonApi.Example

[<RequireQualifiedAccess>]
module ServiceRepository =
    let private servicesDatabase = List.choose (Service.parse "-") [
        "example-app1"
        "example-app3"
        "example-app5"
        "example-app2"
        "example-app4"
    ]

    let find service = async {
        return servicesDatabase |> List.tryFind ((=) service) |> Ok
    }

    let search (searchArgs: ServiceSearchArgs) = async {
        let services =
            match searchArgs.Id with
            | Some id -> servicesDatabase |> List.filter ((=) id)
            | _ -> servicesDatabase

        let sortedServices =
            match searchArgs with
            | { SortBy = Id; SortDescending = true } -> services |> List.sortDescending
            | _ -> services |> List.sort

        let servicesFromOffset =
            sortedServices
            |> List.splitAt searchArgs.Offset
            |> snd

        let limittedServices =
            if servicesFromOffset.Length > searchArgs.Limit then servicesFromOffset |> List.take searchArgs.Limit
            else servicesFromOffset

        return Ok limittedServices
    }

module Services =
    open Felicity

    let define = Define<JsonApiContext, Service, Service>()
    let id = define.Id.ParsedOpt(Service.concat "-", Service.parse "-", id)
    let resourceDef = define.Resource("services", id).CollectionName("services")

    let getCollection =
        define.Operation
            .GetCollection(fun ctx parser ->
                parser.For(ServiceSearchArgs.empty)
                    .Add(ServiceSearchArgs.setId, Filter.Field(id))
                    .Add(ServiceSearchArgs.setSort, Sort.Enum(ServiceSort.fromStringMap))
                    .Add(ServiceSearchArgs.setOffset, Page.Offset)
                    .Add(ServiceSearchArgs.setLimit, Page.Limit.Max(1000))
                    .BindAsyncRes(ServiceRepository.search)
                )

    let lookup =
        define.Operation.LookupAsyncRes(ServiceRepository.find)

    let get =
        define.Operation
            .GetResource()
