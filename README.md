# MermaidMaker

### プロジェクトの説明

Unityプロジェクト内のクラス図を生成し、プロジェクトの設計を視覚化するためのツールです。以下の手順に従って、クラス図を生成することができます。。

## セットアップ

### インストール

以下の手順に従ってインストールしてください。
1. Windows > Package Managerを開く
2. 左上の+ボタンから、Add package from git URLを選択
3. `https://github.com/takuto0404/MermaidMaker.git?path=MermaidMakerProject/Assets/Plugins`を入力してインストール

また、Packages/manifest.jsonに`"c"com.ohagi.mermaidmaker": "https://github.com/takuto0404/MermaidMaker.git?path=MermaidMakerProject/Assets/Plugins","`を追加することでもインストールできます。

### 使い方

正常にインストールが完了すると、Window > MermaidMakerが表示されます。

<p align="center">
  <img width="187" src="https://github.com/takuto0404/MermaidMaker/assets/103303559/62891ffd-5232-40b7-9b69-5250bc576a66">
</p>

開くと、以下のようなWindowが表示されます。

<p align="center">
  <img width="426" src="https://github.com/takuto0404/MermaidMaker/assets/103303559/f2367ba6-2c3a-47f3-a548-714cd9e34f25">
</p>

Select Assemblyを選択すると、指定したAssembly Definition Assetの範囲内から名前空間を検索できるようになります。また、何も選択しなかった場合、名前空間はAssembly-CSharpから検索されます。

また、適用したいC#ファイルが名前空間を持っていない場合は、名前空間を作ってから再度Windowを開くと適用することができます。

Applyを押すと新しいWindowが立ち上がり、選択されたAssembly Definition Assetに応じて名前空間が以下のようにツリー型で表示されるので、クラス図に含めたい名前空間を選択します。

<p align="center">
  <img width="532" src="https://github.com/takuto0404/MermaidMaker/assets/103303559/664c8309-a879-47e1-ae1a-c59584205ad2">
</p>

また、以下のように出力のオプションを選択することができます。 `Generate new file`では、Assetフォルダの中にMermaidMakerフォルダを作り、その中にクラス図の書かれたマークダウンファイルを生成します。`Overwrite an existing file`では、元あるマークダウンファイルを上書きすることができます。`Output to Debug Log`では、コンソールにデバッグログでクラス図の文字列を出力します。

<p align="center">
  <img width="375" src="https://github.com/takuto0404/MermaidMaker/assets/103303559/3f106f58-f863-4a8d-937d-32fd7eb29d92">
</p>

## クラス図について

| 記号 | 属性      | 
| ---- | --------- | 
| +    | public    | 
| -    | private   | 
| #    | protected | 

フィールドは`属性 型 名前`のように表され、メソッドは`属性 型 名前(型 名前...)`というように表されます。また、現段階では、継承の関係のみ矢印(--|>)で表すことが可能です。
型に関して、Int32やBoolean,Singleなどの型はエイリアスのint,bool,floatのように表されます。

## 現段階で未実装の部分
* 関連や実装の矢印の追加
* mdファイルの保存場所の選択
* タプルの名前
* クラスの属性(abstract...)
etc.
