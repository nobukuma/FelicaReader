## FelicaReader Plugin for Xamarin Android/UWP

FelicaReaderは、XamarinのAndroidアプリとUWPアプリでFelicaカード読み取りを行えるプラグインです。

Felicaカードへのコマンド送信は、[Felicaカードユーザーズマニュアル抜粋版](https://www.sony.co.jp/Products/felica/business/tech-support/index.html)に
記載されたコマンドの一部だけを実装しています。特に、暗号化領域に関するコマンドは、技術情報を持ち合わせていないため、今後もサポートする予定はありません。

## プラットフォームのサポート状況

|Platform|Version|
| ------------------- | :------------------: |
|Xamarin.Android|API 14+|
|Windows 10 UWP|10.0.10240+|

## リリースノート

- 201712/29 READMEを改定しました。
- 201712/26 公開しました。

## 使い方

以下では、Xamarin.Formsプロジェクト（アプリ名をSample.FelicaReaderとします）において、変更すべき箇所を説明します。
サンプルコードも公開していますので、併せて参照してください。

Xamarin.Forms sample: [nobukuma/FelicaReaderSample](https://github.com/nobukuma/FelicaReaderSample)

### インストール

プロジェクトに、NuGetからパッケージをインストールします。

- Nuget: [![NuGet Badge](https://buildstats.info/nuget/Plugin.FelicaReader)](https://www.nuget.org/packages/Plugin.FelicaReader)

Xamarin.Formsアプリのソリューションにおいて、 
共通部分のSample.FelicaReader、またSample.FelicaReader.DroidとSample.FelicaReader.UWPの それぞれでFelicaReaderプラグインをNuGetからインストールする必要があります。

## Android固有部分での実装内容

Androidでは、Felicaカードが端末で検知されたときにインテントが発生して、それをアプリで受信することで検知されたFelicaカードを読み取れます。
このインテントの処理は、Androidの固有部分Sample.FelicaReader.Droidで実装する必要があります。

具体的には、Androidの固有部分Sample.FelicaReader.Droidで、ActionTechDiscoveredインテントを受信して、以下を行います。

- インテントのタグデータからNfcFインスタンスを生成する。
- NfcFインスタンスに接続する。
- FelicaCardMediaImplementationインスタンスを作成して、サブスクライバーに通知する。

### MainActivity.cs

MainActivity.csでの実装内容は、以下の通りです。

- クラス属性の設定
    - IntentFilter属性の設定
    - MetaData属性の設定

```C#
namespace Sample.FelicaReader.Droid
{
    [Activity(Label = "Sample.FelicaReader", Icon = "@drawable/icon", MainLauncher = true,
        LaunchMode = Android.Content.PM.LaunchMode.SingleTop,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [IntentFilter(new[] { NfcAdapter.ActionTechDiscovered })]
    [MetaData(NfcAdapter.ActionTechDiscovered, Resource = "@xml/nfc_filter")]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private IFelicaReader felicaReader;
```

- OnCreateでの設定
    - MainActivityインスタンスとその型を指定して、CrossFelicaReader.Initを呼び出す
    - インテントの処理（タグデータからNfcFインスタンスを生成して、接続、それを指定して作成したFelicaCardMediaImplementationインスタンスをサブスクライバーに通知)

```C#
protected override void OnCreate(Bundle bundle)
{
    ...(省略)...

    CrossFelicaReader.Init(this, GetType());
    this.felicaReader = CrossFelicaReader.Current;
    LoadApplication(new App(new AndroidInitializer()));

    this.ProcessActionTechDiscoveredIntent(this.Intent);
}

private void ProcessActionTechDiscoveredIntent(Intent intent)
{
    string action = intent.Action;
    if (action != NfcAdapter.ActionTechDiscovered)
    {
        return;
    }

    var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Android.Nfc.Tag;
    if (tag != null)
    {
        var subject = this.felicaReader.WhenCardFound() as Subject<IFelicaCardMedia>;
        NfcF nfc = NfcF.Get(tag);
        nfc.Connect();
        subject.OnNext(new FelicaCardMediaImplementation(nfc));
    }
}
```

- OnNewIntentでのインテントの処理
    - 処理内容は上記のインテントの処理と同じ

```C#
protected override void OnNewIntent(Intent intent)
{
    base.OnNewIntent(intent);
    ProcessActionTechDiscoveredIntent(intent);
    return;
}
```

- Pause・Resume時のフォアグラウンドでのインテントのディスパッチの停止・開始

```C#
protected override void OnPause()
{
    base.OnPause();
    this.felicaReader.DisableForeground();
}

protected override void OnResume()
{
    base.OnResume();
    this.felicaReader.EnableForeground();
}
```

### Resrouces/xml/nfc_filter.xml

Resrouces/xml/nfc_filter.xmlを作成して、以下を記述します。

```xml
<?xml version="1.0" encoding="utf-8"?>
<resources xmlns:xliff="urn:oasis:names:tc:xliff:document:1.2" >
  <tech-list>
    <tech>android.nfc.tech.NfcF</tech>
  </tech-list>
</resources>
```

### プロジェクトのプロパティ

プロジェクトのプロパティを開いて、AndroidマニフェストのRequired PermissionsでNFCを選択します。

## UWP固有部分での実装内容

UWPの固有部分Sample.FelicaReader.UWPで実装すべきコードはありません。

### パッケージマニフェスト

Sample.FelicaReader.UWPのパッケージマニフェストを開いて、機能シートにおいて近接通信を選択します。

## 共通部分での実装内容

共通部分Sample.FelicaReaderでは、Felicaカード検知の通知を購読して、その処理を記述します。

サンプルでは、MainPageのViewModelで、Felicaカード検知の通知を購読して、検知したときの処理を書いています。
ReadWithoutEncryptionで指定するサービスコードとブロック番号リストを変えることで、SuicaやEdyなどFelicaカードが実装するサービスからデータを取得できます。

```C#
public class MainPageViewModel : BindableBase, INavigationAware
{
    private IFelicaReader felicaReader;
    private IDisposable subscription;
    
    ...

    public MainPageViewModel()
    {
        this.felicaReader = CrossFelicaReader.Current;
        ...
    }
    
    ...

    public void OnNavigatedFrom(NavigationParameters parameters)
    {
        this.subscription.Dispose();
    }
    
    ...

    public  void OnNavigatedTo(NavigationParameters parameters)
    {
        this.subscription = this.felicaReader.WhenCardFound().Subscribe(async x =>
        {
            try
            {
                var byteIdm = await x.GetIdm();
                this.IDmString = BitConverter.ToString(byteIdm);
                System.Diagnostics.Debug.WriteLine("Idm: {0}", this.IDmString, 0x00);

                var result = await x.ReadWithoutEncryption(byteIdm, 0x008b, 1, new byte[] { 0x80, 0x00 });
                string resStr = BitConverter.ToString(result.PacketData);
                System.Diagnostics.Debug.WriteLine("Res: {0}(len={1})", resStr, result.PacketData.Length);

                this.Message = String.Format("Res: {0}", resStr, 0x00);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            finally
            {
                x.Dispose();
            }
        },
        onError: x =>
        {
            System.Diagnostics.Debug.WriteLine(x.Message);
        });
    }
}
```

なお、UWPでアプリが利用される場合、共通部分でFelicaカード検知の開始処理を書く必要があります。サンプルでは、ボタンが押されたときにFelicaカードの検知を開始しています。

```C#
public class MainPageViewModel : BindableBase, INavigationAware
{
    private IFelicaReader felicaReader;

    ...

    public DelegateCommand ReadButtonClickedCommand { get; private set; }

    public MainPageViewModel()
    {
        this.felicaReader = CrossFelicaReader.Current;
        this.ReadButtonClickedCommand = new DelegateCommand(() => ButtonClicked());
        ...
    }

       private void ButtonClicked()
    {
        this.felicaReader.FindCard();
    }
```

## 提供するAPI

FelicaReaderプラグインでは、以下の２つのインターフェースを提供します。

凡例
- ○　実装済み
- △　実装を予定
- ー　実装せず

## IFelicaReaderインターフェース

Felicaカードを検知したときに、アプリが通知を受け取るための機能を提供します。

API | 説明 | Android | UWP
---|---|---|---
EnableForeground | フォアグラウンドでのカード検知の有効設定 | ○ | △
DisableForeground | フォアグラウンドでのカード検知の無効設定 | ○ | △
FindCard | カード検知の開始 | ー | ○
IsSupported | Felicaカード読み取り機能の対応状況の取得 | ○ | ○
IsEnabled | Felicaカード読み取り機能の有効・無効状態の取得 | ○ | ○
WhenCardFound | Felicaカード検知の通知を登録するIObservableの取得 | ○ | ○

WhenCardfoundで返されるIObservableインスタンスのサブスクライバーは、Felicaカードを検知すると、IFelicaCardMediaインターフェースを実装したクラスのインスタンスを受け取ります。


## IFelicaCardMediaインターフェース

検知したFelicaカードの情報を取得するための機能を提供します。
Felicaカードへのコマンド送信は、[Felicaカードユーザーズマニュアル抜粋版](https://www.sony.co.jp/Products/felica/business/tech-support/index.html)に
記載されたコマンドの一部だけを実装しています。

API | 説明 | Android | UWP
---|---|---|---
GetIdm | 検知したFelicaカードのIDm取得 | ○ | ○
Send | 検知したFelicaカードへの任意のバイトデータの送信 | ○ | ○
Polling | Pollingコマンドの送信、応答の解析 | ○ | ○
ReadWithoutEncryption | ReadWithoutEncryptionコマンドの送信、応答の解析 | ○ | ○
RequestService | RequestServiceコマンドの送信、応答の解析 | ○ | ○
RequestSystemCode | RequestSystemCodeコマンドの送信、応答の解析 | ○ | ○
RequestResponse | RequestResponseコマンドの送信、応答の解析 | ○ | ○
SearchServiceCode | SearchServiceCodeコマンドの送信、応答の解析 | ○ | ○

## ライセンス

[LICENSE](./LICENSE)を参照してください。

## クレジット

プラグインに含まれるPcscSdkプロジェクトは、
[Universal Windows Platform (UWP) app samples](https://github.com/Microsoft/Windows-universal-samples)
のNear field communication (NFC) sample(Samples/Nfc)に含まれるライブラリを修正したものです。
