// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE file in the project root for full license information.

function toggleMenu() {
    var sidebar = document.getElementById("sidebar");
    var blackout = document.getElementById("blackout");

    if (sidebar.style.left === "0px")
    {
        sidebar.style.left = `-${sidebar.offsetWidth}px`;
        blackout.classList.remove("showThat");
        blackout.classList.add("hideThat");
    }
    else
    {
        sidebar.style.left = "0px";
        blackout.classList.remove("hideThat");
        blackout.classList.add("showThat");
    }
}