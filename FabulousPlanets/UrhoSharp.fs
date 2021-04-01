namespace FabulousPlanets

open Fabulous
open Fabulous.XamarinForms
open Urho
open Urho.Forms

[<AutoOpen>]
module UrhoSharpExtensions =
    type UrhoApplicationOptions =
        { AssetsFolder: string option
          Orientation: ApplicationOptions.OrientationType option
          HighDpi: bool option }

    let createApplicationOptions (value: UrhoApplicationOptions) =
        let assetsFolder = match value.AssetsFolder with None -> null | Some folder -> folder                    
        let options = ApplicationOptions(assetsFolder)
        match value.Orientation with None -> () | Some orientation -> options.Orientation <- orientation
        match value.HighDpi with None -> () | Some highDpi -> options.HighDpi <- highDpi
        options

    let OptionsAttribKey = AttributeKey<_> "UrhoSurface_Options"
    let CreatedAttribKey<'T when 'T :> Urho.Application> = AttributeKey<('T -> unit)> "UrhoSurface_Created"

    type Fabulous.XamarinForms.View with
        static member UrhoApplicationOptions(?assetsFolder: string, ?orientation: ApplicationOptions.OrientationType, ?highDpi: bool) =
            { AssetsFolder = assetsFolder
              Orientation = orientation
              HighDpi = highDpi }

        static member inline UrhoSurface<'T when 'T :> Urho.Application>(?options: UrhoApplicationOptions, ?created: ('T -> unit),
                                                                         // inherited attributes common to all views
                                                                         ?horizontalOptions, ?verticalOptions, ?margin, ?gestureRecognizers, ?anchorX, ?anchorY, ?backgroundColor,
                                                                         ?height, ?inputTransparent, ?isEnabled, ?isVisible, ?minimumHeight, ?minimumWidth,
                                                                         ?opacity, ?rotation, ?rotationX, ?rotationY, ?scale, ?style, ?translationX, ?translationY, ?width,
                                                                         ?resources, ?styles, ?styleSheets, ?classId, ?styleId, ?automationId) =

            let attribCount = match options with Some _ -> 1 | None -> 0
            let attribCount = match created with Some _ -> attribCount + 1 | None -> attribCount

            let attribs = 
                ViewBuilders.BuildView(attribCount, ?horizontalOptions=horizontalOptions, ?verticalOptions=verticalOptions, 
                               ?margin=margin, ?gestureRecognizers=gestureRecognizers, ?anchorX=anchorX, ?anchorY=anchorY, 
                               ?backgroundColor=backgroundColor, ?height=height, ?inputTransparent=inputTransparent, 
                               ?isEnabled=isEnabled, ?isVisible=isVisible, ?minimumHeight=minimumHeight,
                               ?minimumWidth=minimumWidth, ?opacity=opacity, ?rotation=rotation, 
                               ?rotationX=rotationX, ?rotationY=rotationY, ?scale=scale, ?style=style, 
                               ?translationX=translationX, ?translationY=translationY, ?width=width, 
                               ?resources=resources, ?styles=styles, ?styleSheets=styleSheets, ?classId=classId, ?styleId=styleId, ?automationId=automationId)

            match options with None -> () | Some v -> attribs.Add(OptionsAttribKey, v)
            match created with None -> () | Some v -> attribs.Add(CreatedAttribKey, v)

            let updateApplicationOptions (target: UrhoSurface) v =
                let updateAsync = (async { 
                    let applicationOptions = createApplicationOptions v
                    let! application = target.Show<'T>(applicationOptions) |> Async.AwaitTask
                    match created with
                    | None -> ()
                    | Some func -> func application
                })
                updateAsync |> Async.StartImmediate

            let update (prevOpt: ViewElement voption) (source: ViewElement) (target: UrhoSurface) =
                ViewBuilders.UpdateView (prevOpt, source, target)
                source.UpdatePrimitive(prevOpt, target, OptionsAttribKey, updateApplicationOptions)
                
            let updateAttachedProperties propertyKey prevOpt curr target =
                ViewBuilders.UpdateViewAttachedProperties(propertyKey, prevOpt, curr, target)

            ViewElement.Create(UrhoSurface, update, updateAttachedProperties, attribs)