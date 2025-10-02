import { defineConfig } from "vitepress";
import * as en_us from "./en-us.mts";
import * as ja_jp from "./ja-jp.mts";

export default defineConfig(
{
  cleanUrls: true,
  rewrites:
  {
    "en-us/:rest*": ":rest*"
  },
  locales:
  {
    "root": en_us.main,
    "ja-jp": ja_jp.main,
  },
  themeConfig:
  {
    socialLinks:
    [
      {
        icon: "github",
        link: "https://github.com/nekobako/BlendShapeModifier",
      },
    ],
    search:
    {
      provider: "local",
      options:
      {
        locales:
        {
          "root": en_us.search,
          "ja-jp": ja_jp.search,
        },
      },
    },
  },
});
