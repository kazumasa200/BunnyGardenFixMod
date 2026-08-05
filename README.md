# BunnyGardenFixMod
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/6ad0f502-037b-42dd-a276-f7b1f230e99e" />


[バニーガーデン](https://store.steampowered.com/app/2654470/_/)(海外名:Bunny Garden)用の解像度修正やフレームレート上限変更などを行うBepInEx5用Modです。

## おしらせ
バージョン1.1.0から、**フリーカメラ**と**ゲーム中にF9キーで開ける設定パネル**を追加しました！！  
今までのようにメモ帳で設定ファイルを書き換えなくても、ゲームを起動したまま設定を変更できます！！  
時間停止やスクリーンショット機能も追加しました～  

バージョン1.0.1から、BepInEx6にも対応しました！！  
今までのBepInEx5版もひきつづき開発します！！  
また、下で紹介している導入方法はBepInEx5のものになります！ご了承ください～  

## 対応バージョン(v1.1.0現在)
- ゲームバージョン1.0.6のみ対応  

## 機能

### 画質・パフォーマンス
- 内部解像度を指定することで画質を向上することができる。
- 本来は60で固定されていたフレームレート制限を任意の値にするか、取り払うことができる。
- アンチエイリアシング(Off / FXAA / TAA / MSAA2x〜8x)を変更できる。

### フリーカメラ
**F5**キーでフリーカメラに切り替えることができ、フリーカメラ中に**F6**キーを押すとカメラを固定してキャラ操作ができる。  
ゲームパッドの場合は**Select + Y**でON/OFF、**Select + X**で固定の切り替えです。

| 入力 | 動作 |
|------|------|
| **WASD / 矢印キー** | 前後左右に移動 |
| **Q / E** | 上下に移動 |
| **Shift / Ctrl** | 高速移動 / 低速移動 |
| **マウス** | 視点移動(左右クリックでカーソル固定の切り替え) |
| **左スティック** | 前後左右に移動 |
| **右スティック** | 視点移動 |
| **ZL / ZR** | 下 / 上に移動 |
| **L / R** | 低速移動 / 高速移動 |

フリーカメラ中は誤操作を防ぐため、ゲーム本体のUIを自動的に隠します(設定でOFFにもできます)。ポーズメニューや確認ダイアログが出たときは自動的にUIが戻ります。

### 時間停止・スロー・早送り
- **T**キーで時間停止、**Y**キーでスロー再生、**G**キーを押している間だけ早送りになります。
- スローと早送りの速さは設定から変更できます。

### スクリーンショット
- **P**キー(ゲームパッドは**Select + A**)で、ゲームUIやModのオーバーレイを写さずにPNGを保存します。
- 解像度の倍率も設定できます。保存先は```BepInEx/screenshots/net.noeleve.BunnyGardenFixMod/```です。

### そのほか
- 起動時に最新版があるかを確認して、更新があればお知らせします(設定でOFFにできます)。
- タイトル画面にModのバージョンを表示します。
- ゲームの言語設定に合わせて、Modの表示も日本語/英語が切り替わります。

## 設定方法
ゲーム中に**F9**キーを押すと設定パネルが開きます。ほとんどの設定はここから変更できて、変更した内容はすぐ反映＆自動保存されます。

- **↑↓**で項目の移動、**←→**で値の変更、**Space**でON/OFF切り替え、**Tab**でカテゴリ切り替え、**Esc**か**F9**で閉じる
- ホットキーの割り当ても変更できます(キーボード・ゲームパッドどちらも)
- 「初期値に戻す」ボタンでカテゴリごとに既定値へ戻せます

もちろん今までどおり```BepInEx/config/net.noeleve.BunnyGardenFixMod.cfg```を直接編集してもOKです。設定項目の一覧は[docs/configs.md](docs/configs.md)にまとまっています。

## 導入方法(Steam Deck対応)
1. [Releases](https://github.com/kazumasa200/BunnyGardenFixMod/releases)から最新のzipファイルをダウンロードする。(BunnyGardenFixMod_v1.1.0_BepInEx5.zipみたいな感じ)ブラウザによってはブロックするかもしれないので注意。<br>例<img width="1522" height="610" alt="image" src="https://github.com/user-attachments/assets/d8dc6b24-b9d8-4cf3-8b24-6fce6c07a975" />
<br>画像はV1.0.0の例です。導入時の最新バージョンを入れてください。
1. [BepInEx5](https://github.com/bepinex/bepinex/releases)をダウンロードする。Windowsの場合もSteam Deckの場合も```BepInEx_win_x64_{バージョン名}.zip```をダウンロードする。
1. ゲームのexeがあるディレクトリにBepInEx5の中身を展開。つまり、BUNNY GARDEN.exeとBepInExフォルダやdoorstop_configとかが同じ階層にある状態が正しいということ。<br>例<img width="1362" height="597" alt="image" src="https://github.com/user-attachments/assets/fc262c84-3954-48c5-a37f-bb29b08de2f3" />
1. (Steam Deckの場合のみ実行) Steamでバニーガーデン → 右クリック → 「プロパティ」→「一般」→「起動オプション」に```WINEDLLOVERRIDES="winhttp=n,b" %command%```を入力。
1. 一度ゲームを起動した後、[Releases](https://github.com/kazumasa200/BunnyGardenFixMod/releases)からダウンロードしたZipを展開し、中にある```net.noeleve.BunnyGardenFixMod.dll```をBepinExフォルダの中にPluginsの中に入れる。<br>例<img width="1492" height="536" alt="image" src="https://github.com/user-attachments/assets/aaaa2519-19b4-4c20-bfb6-c33c8ff6423f" />


1. もう一度起動して、**F9**キーで設定パネルが開けば導入完了です！ここから解像度やフレームレートなどの設定ができます。<br>(設定ファイルを直接いじりたい場合は、BepinExフォルダの中のconfigフォルダにできる```net.noeleve.BunnyGardenFixMod.cfg```をメモ帳などで変更してください)<br>例<img width="1218" height="732" alt="image" src="https://github.com/user-attachments/assets/a4a7e6bf-5af0-4770-b042-f9446ef8c443" />



## 既知の問題点
[Issues](https://github.com/kazumasa200/BunnyGardenFixMod/issues)をご確認ください。バグや改善点、ほしい機能ありましたら[Issues](https://github.com/kazumasa200/BunnyGardenFixMod/issues)もしくは[X](https://x.com/kazumasa200)までお願いします。  
要望の際は右上のNew Issueから個別のissueを作ってください。

## ライセンス
このModは**MITライセンス**で公開しています。詳しくは[LICENSE](LICENSE)をご覧ください。  
加えて、原作の開発元である**株式会社qureate**に対しては、このModのソースコードの一部または全部を(クローズドソースの本編を含む)製品へ組み込むことを、著作権表示の義務なしで許諾しています。

## 注意
最後に、このModを使用して配信することで動画プラットフォームからBAN等の処置を受けられた際も一切の責任を持ちません。自己責任。

## お問い合わせ
X(旧Twitter):@kazumasa200
