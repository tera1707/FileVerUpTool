## 概要

バージョン一括確認・更新ツール

ソリューションが抱えるプロジェクトの数が多くて、リリースのたびにバージョン更新するのが大変＆抜け漏れ心配だったので作ったツール。


## 使い方

### 基本

アプリを起動する

一番上のテキストボックスに、対象のプロジェクトを含むフォルダのパスを入れる。

「Read csproj」ボタンを押す。
(csprojだけじゃないけど)

バージョン等の情報が、リストで表示される。

リスト内の編集したい値をダブルクリックして編集する。

編集が終わったら、「Write csproj」ボタンを押す。


### 一括設定

画面下部の「Ikkatsu Settei」で、全PJ分の項目をまとめて設定できる。

「Version」と出てるコンボボックスを押す。

設定項目がずらっとでるので、一括設定したい項目を選ぶ。

その下のテキストボックスに、選んだ項目に設定したい値を入力する。

「Ikkatsu Settei Button」を押す。

全PJの指定の項目に、今入れた値がはいる。
(この時点ではリストが更新されただけで、まだ実際に設定変更は行われていない)

「Write csproj」を押す。


## やってること

プロジェクトのバージョンなどの情報を保存したファイルを検索して、そこに記載された各種情報をリスト表示する。

- .net5以降のPJ
  - <プロジェクト名>.cprojファイル
- .net FrameworkのPJ
  - AssemblyInfo.cs
- C++のプロジェクト
  - <プロジェクト名>.rcファイル

値を設定する際は、各ファイルに書かれた情報の部分を、指定の値で置換する。

