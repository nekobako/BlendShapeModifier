export const main =
{
  lang: "en-us",
  label: "English",
  title: "Blend Shape Modifier",
  description: "A non-destructive tool to add and edit blend shapes for VRChat avatars",
  themeConfig:
  {
    nav:
    [
      {
        text: "Home",
        link: "/",
      },
      {
        text: "Getting Started",
        link: "/getting-started/",
        activeMatch: "/getting-started/",
      },
      {
        text: "Tutorials",
        link: "/tutorials/",
        activeMatch: "/tutorials/",
      },
      {
        text: "References",
        link: "/references/",
        activeMatch: "/references/",
      },
    ],
    sidebar:
    [
      {
        text: "Getting Started",
        link: "/getting-started/",
        collapsed: false,
        items:
        [
          {
            text: "Installation",
            link: "/getting-started/installation",
          },
          {
            text: "Setup",
            link: "/getting-started/setup",
          },
        ],
      },
      {
        text: "Tutorials",
        link: "/tutorials/",
        collapsed: false,
        items:
        [
          {
            text: "Beyond Limits",
            link: "/tutorials/beyond-limits",
          },
          {
            text: "Merge and Filter",
            link: "/tutorials/merge-and-filter",
          },
          {
            text: "Multi-Frames",
            link: "/tutorials/multi-frames",
          },
          {
            text: "Animations",
            link: "/tutorials/animations",
          },
        ],
      },
      {
        text: "References",
        link: "/references/",
        collapsed: false,
        items:
        [
          {
            text: "Blend Shape Modifier Component",
            link: "/references/blend-shape-modifier-component",
          },
          {
            text: "Shape",
            link: "/references/shape",
          },
          {
            text: "Frame",
            link: "/references/frame",
          },
          {
            text: "Expressions",
            link: "/references/expressions/",
            collapsed: false,
            items:
            [
              {
                text: "Sample Expression",
                link: "/references/expressions/sample-expression",
              },
              {
                text: "Merge Expression",
                link: "/references/expressions/merge-expression",
              },
              {
                text: "Filter By Axis Expression",
                link: "/references/expressions/filter-by-axis-expression",
              },
              {
                text: "Filter By Mask Expression",
                link: "/references/expressions/filter-by-mask-expression",
              },
            ],
          },
        ],
      },
    ],
  },
};

export const search =
{
  translations:
  {
    button:
    {
      buttonText: "Search",
    },
    modal:
    {
      displayDetails: "Display details",
      resetButtonTitle: "Reset search",
      backButtonTitle: "Close search",
      noResultsText: "No results",
      footer:
      {
        navigateText: "Navigate",
        selectText: "Select",
        closeText: "Close",
      },
    },
  },
};
