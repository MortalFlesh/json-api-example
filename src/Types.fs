namespace MF.JsonApi.Example

//
// Simple types
//

[<AutoOpen>]
module ServiceTypes =
    type Domain = Domain of string
    type Context = Context of string

    type Service = {
        Domain: Domain
        Context: Context
    }

    [<RequireQualifiedAccess>]
    module Service =
        let parse (separator: string) = function
            | null | "" -> None
            | value ->
                match value.Split(separator, 2) with
                | [| domain; context |] -> Some { Domain = Domain domain; Context = Context context }
                | _ -> None

        let concat separator { Domain = Domain domain; Context = Context context } =
            $"{domain}{separator}{context}"

    [<RequireQualifiedAccess>]
    type ServiceSort =
        | Id

        static member fromStringMap = [
            "id", Id
        ]

    type ServiceSearchArgs = {
        Id: Service option

        SortBy: ServiceSort
        SortDescending: bool
        Offset: int
        Limit: int
    }

    [<RequireQualifiedAccess>]
    module ServiceSearchArgs =
        let empty = {
            Id = None

            SortBy = ServiceSort.Id
            SortDescending = false
            Offset = 0
            Limit = 100
        }

        let setId id (args: ServiceSearchArgs) = { args with Id = Some id }

        let setSort (sortBy, sortDesc) (args: ServiceSearchArgs) = { args with SortBy = sortBy; SortDescending = sortDesc }
        let setOffset offset (args: ServiceSearchArgs) = { args with Offset = offset }
        let setLimit limit (args: ServiceSearchArgs) = { args with Limit = limit }
