var sidebarViewport = document.querySelector('nav#sidebar .os-viewport');

var sidebarChildren = sidebarViewport.querySelector('.sidebar').children;
var targetSidebar = sidebarChildren[sidebarChildren.length - 1];

if(!targetSidebar.querySelector('.sidebar-item.active'))
{
    let current;
    const elements = Array.from(targetSidebar.querySelectorAll("a.sidebar-item[href]")).filter(element => element.getAttribute("href").includes("/../"));

    if(elements.length > 0)
    {
        current = $(elements[0]);
    }

    while(current.length > 0) {
        current.addClass('active');
        current = current.parents('.sidebar-item');
    }
}

var observerSidebar = new MutationObserver(function (list, observer) {
    var activeTocItems = targetSidebar.querySelectorAll('.sidebar-item.active');

    if(activeTocItems.length > 0){
        var activeTocItem = activeTocItems[activeTocItems.length - 1];
        var activeTocItemRect = activeTocItem.getBoundingClientRect();
        var viewportRect = sidebarViewport.getBoundingClientRect();

        sidebarViewport.scrollTo({
            top: (activeTocItemRect.top - viewportRect.top) + (activeTocItemRect.height - viewportRect.height) / 2
        });

        var navItems = document.querySelectorAll("nav #toc li");

        for(var indexNavItem = 0; indexNavItem < navItems.length; indexNavItem++)
        {
            var navItem = navItems[indexNavItem];
            var navLink = navItem.querySelector("a");
            if(navLink != null && navLink.textContent == "Explicit Interface Implementations")
            {
                navItem.parentElement.removeChild(navItem);
            }
        }

        observer.disconnect();
    }
});

observerSidebar.observe(targetSidebar, { attributes: true, childList: true, subtree: true });