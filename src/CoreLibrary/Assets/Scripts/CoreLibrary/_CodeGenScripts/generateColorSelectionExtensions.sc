// This scala script is used to generate the source of VectorSelectionExtensions.cs
// You need either the scala interpreter or the ammonite shell to run this. 

val values = Seq("r", "g", "b", "a")
val res = (for {
    one <- values
    two <- values
    three <- values
    four <- values
    name = one + two + three + four
    returnType = "Color"
    source = "Color"
} yield s"""
        |/// <summary> 
        |/// Returns a new ${returnType} with it's values set to 
        |/// the ${name}-values of the passed color in that order.
        |/// </summary>
        |public static ${returnType} ${name}(this ${source} col)
        |{
        |   return new ${returnType}(${name.map(arg => s"col.${arg}").mkString(", ")});    
        |}
        """.stripMargin).mkString

println(res)