---
title: "The Unity CoreLibrary"
author: [Cameron Reuschel]
subject: "CoreLibrary"
subtitle: "A collection of useful classes and extension methods for every Unity project"
date: "Version 0.0.1"
titlepage: true
...

\newpage

# Introduction

My name is Cameron Reuschel, a student of Computer Science and Games Engineering at the university of Würzburg, Germany. All of this started during my first year of Games Engineering, were we had to develop a small game in teams of three, with no previous expertise whatsoever.

Unlike most of my peers, my more than two years of Computer Science as well as experience tutoring for the Java programming course gave me plenty of knowledge about programming in general. During the course of the one-year project, I gained even more knowledge through my work as a software developer for Java Enterprise technology and my undying love for functional programming.

As far as Unity was concerned, I quickly grew uneasy with the way I had to write code in order to make things happen the way I wanted them to happen. Things like `GameObject.SendMessage()` and the non-existence of `transform.GetChildren()` made my software engineer heart bleed a little. It seemed like Unity abused the statically typed, compiled and enterprise-ready C# language so that anyone could write an unmaintainable mess of code.

Another issue is the way Unity abuses C#'s *yield return* construct for some sort of *same-thread-parallelism*. Don't get me wrong, I love coroutines. But the fact that C# does not support `async / await`, `ref` parameters or anonymous declarations of IEnumerators made me doubt my sanity.

Many of the classes and methods provided with the CoreLibrary started out as little helpers I wrote for myself in that first-year game project, and my two teammates quickly learned to appreciate all the little quality-of-life improvements.

When we started the [CaveLands](http://cavelands.de/) project, some of my colleagues quickly stumbled across the same shortcomings that I encountered some time ago. This led to me creating the `CoreLibrary`, an independent project containing all these little utilities in a *well-tested*, *well-documented* and polished form for everyone to use.

After a semester of internal usage in the CaveLands project, other Games Engineering students showed interest in using the `CoreLibrary` as well. So we decided to open source the project under the MIT license for everyone to use.

Fell free to add any issue, forgotten edge cases or feature wishes as an [Issue on GitHub](???).\
I'll also gladly accept any pull requests.

**Enjoy.**
