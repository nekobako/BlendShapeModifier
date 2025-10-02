export const main =
{
  lang: "ja-jp",
  label: "日本語",
  title: "ブレンドシェイプモディファイヤー",
  description: "VRChat アバターのための非破壊ブレンドシェイプ追加/編集ツール",
  themeConfig:
  {
    nav:
    [
      {
        text: "ホーム",
        link: "/ja-jp/",
      },
      {
        text: "はじめに",
        link: "/ja-jp/getting-started/",
        activeMatch: "/ja-jp/getting-started/",
      },
      {
        text: "チュートリアル",
        link: "/ja-jp/tutorials/",
        activeMatch: "/ja-jp/tutorials/",
      },
      {
        text: "リファレンス",
        link: "/ja-jp/references/",
        activeMatch: "/ja-jp/references/",
      },
    ],
    sidebar:
    [
      {
        text: "はじめに",
        link: "/ja-jp/getting-started/",
        collapsed: false,
        items:
        [
          {
            text: "インストール",
            link: "/ja-jp/getting-started/installation",
          },
          {
            text: "セットアップ",
            link: "/ja-jp/getting-started/setup",
          },
        ],
      },
      {
        text: "チュートリアル",
        link: "/ja-jp/tutorials/",
        collapsed: false,
        items:
        [
          {
            text: "限界突破",
            link: "/ja-jp/tutorials/beyond-limits",
          },
          {
            text: "合成/分割",
            link: "/ja-jp/tutorials/merge-and-filter",
          },
          {
            text: "マルチフレーム",
            link: "/ja-jp/tutorials/multi-frames",
          },
          {
            text: "アニメーション",
            link: "/ja-jp/tutorials/animations",
          },
        ],
      },
      {
        text: "リファレンス",
        link: "/ja-jp/references/",
        collapsed: false,
        items:
        [
          {
            text: "Blend Shape Modifier コンポーネント",
            link: "/ja-jp/references/blend-shape-modifier-component",
          },
          {
            text: "シェイプ",
            link: "/ja-jp/references/shape",
          },
          {
            text: "フレーム",
            link: "/ja-jp/references/frame",
          },
          {
            text: "エクスプレッション",
            link: "/ja-jp/references/expressions/",
            collapsed: false,
            items:
            [
              {
                text: "Sample エクスプレッション",
                link: "/ja-jp/references/expressions/sample-expression",
              },
              {
                text: "Merge エクスプレッション",
                link: "/ja-jp/references/expressions/merge-expression",
              },
              {
                text: "Filter By Axis エクスプレッション",
                link: "/ja-jp/references/expressions/filter-by-axis-expression",
              },
              {
                text: "Filter By Mask エクスプレッション",
                link: "/ja-jp/references/expressions/filter-by-mask-expression",
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
      buttonText: "検索",
    },
    modal:
    {
      displayDetails: "詳細を表示",
      resetButtonTitle: "検索をリセット",
      backButtonTitle: "検索を閉じる",
      noResultsText: "結果がありません",
      footer:
      {
        navigateText: "移動",
        selectText: "選択",
        closeText: "閉じる",
      },
    },
  },
};
