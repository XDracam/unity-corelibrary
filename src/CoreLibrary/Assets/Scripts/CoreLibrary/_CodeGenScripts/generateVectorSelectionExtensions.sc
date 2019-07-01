// This scala script is used to generate the source of VectorSelectionExtensions.cs
// You need either the scala interpreter or the ammonite shell to run this. 

val vec2 = "Vector2"
val vec3 = "Vector3"
val vec4 = "Vector4"
val coords = Seq("", "x", "y", "z", "w")
val methods = for {
    one <- coords
    two <- coords
    three <- coords
    four <- coords
    name = one + two + three + four
    if name.length > 1
    returnType = name.length match {
        case 2 => vec2
        case 3 => vec3
        case 4 => vec4
    }
    source <- if (name.contains('w')) Seq(vec4)
              else if (name.contains('z')) Seq(vec3, vec4)
              else Seq(vec2, vec3, vec4)
    argOrder = name.toLowerCase
} yield (returnType, name, source) 

val res = (for {
    (returnType, name, source) <- methods.distinct
    argOrder = name.toLowerCase
} yield s"""
        |/// <summary> 
        |/// Returns a new ${returnType} with it's coordinates set to 
        |/// the ${argOrder}-coordinates of the passed vector in that order.
        |/// </summary>
        |public static ${returnType} ${name}(this ${source} vec)
        |{
        |   return new ${returnType}(${argOrder.map(arg => s"vec.${arg}").mkString(", ")});    
        |}
        """.stripMargin
).mkString

println(res)