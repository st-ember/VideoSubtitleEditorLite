## 事前準備

在開始前，要完成安裝的軟體和套件：
- 如果要測試 Docker，會需要使用 Windows 11。
- npm
- Visual Studio 2022 17.0 以上，或是更新的 Visual Studio。
- Visual Studio 的 「Bundler & Minifier 2022+」套件 ( https://marketplace.visualstudio.com/items?itemName=Failwyn.BundlerMinifier64 )，當然如果有更新的版本也可以。
- 用來開發 Typescript 的工具，您可以繼續用 Visual Studio 來開發，或是額外安裝 Visual Studio Code (推薦)。
- 啟用 WSL
- 安裝 Docker Desktop (安裝時記得選用 WSL 來執行 Container)

再來是前端 Typescript 專案在開發時會用到的表單元件及頁面專案，這些專案應該已經附在本文件的上一層目錄中 (uform-src)，所有需要的專案清單如下：
- uform-utility
- uform-api
- uform-dialog
- uform-dropdown
- uform-popover
- uform-form
- uform-form-selector
- uform-form-datepicker
- uform-form-time
- uform-form-keycode
- uform-page

找到上面這些專案的路徑後，有三個檔案需要修改：
- /src/SubtitleEditor.Web/Typescripts/SubtitleEditor/package.json
- /src/SubtitleEditor.Web/Typescripts/App/package.json
- /src/SubtitleEditor.Web/Typescripts/Login/package.json

這三個檔案會包含 Typescript 會用到的套件相依性路徑，路徑會放在檔案內的 dependencies 與 devDependencies 區塊，請確定路徑都正確無誤。
完成確認後，您可以在這三個檔案所在位置開啟終端機，並執行下列指令：
```
npm install
```

如果路徑都正確，您可以在與檔案同一層的資料夾下找到 node_modules 資料夾，其內將包含所有相依性專案的捷徑。如果路徑不正確，您會看到大量的錯誤在指令執行後拋出，請確定從 package.json 的該層目錄開始可以正確找到所需的專案，或是直接提供絕對路徑。

## 建置專案

實際發佈專案前，您需要先建置 Typescript 來產生 js 檔，接著執行打包及壓縮，最後才能與完成建置的專案一起發佈。

### 建置 Typescript

開始前請務必確保您已完成所有相依性專案的設定，接著請在三個 Typescript 專案下找到 tsconfig.json 檔，他會為在專案資料夾的 src 目錄底下。三個 Typescript 專案有相依性，所以請依照下列順序來執行本步驟：
- SubtitleEditor
- App
- Login

找到 tsconfig.json 檔後，開啟終端機並輸入指令：
```
tsc -p "tsconfig.json 的絕對路徑"
```

依照 tsconfig.json 內的設定，三個 Typescript 專案會將他們的程式編譯並輸出在專案資料夾的 dist 資料夾內 (src 的隔壁)。

### 打包及壓縮

完成 js 檔的建置後，就可以開始打包及壓縮所有 js 與 css 檔了。
您可以在 SubtitleEditor.Web 專案下找到 bundleconfig.json，請在 Visual Studio 的方案總館內找到這個檔案，並在檔案上按下滑鼠右鍵，選擇「Bundler & Minifier > Update Bundles」。
「Bundler & Minifier」套件會依照 bundleconfig.json 內的設定對所有 js 及 css 打案進行打包與壓縮。

### 修改版本號

系統的版本號不會自動增加，要修改 appsettings.json、appsettings.Staging.json 和 appsettings.Development.json 內的 `Version` 區塊。建議有比較大的變動，或是完成一次發佈後就增加一次版本號。

### 建置 SubtitleEditor C# 專案

使用 Visual Studio 開啟本方案後，在方案總管找到 SubtitleEditor.Web 專案，並在該專案上點右鍵，選擇「建置」來開始編譯。

## 打包成 Docker Image

打包 Image 時可以指定版本號，一般來說建議設定與 appsettings.json 內相同的版本號，可以避免混淆。

在方案資料夾開啟終端機，然後輸入以下指令依照目前的 Dockerfile 步驟，將方案製作成 Docker Image (請自行替換指令內的版本號)：
```
docker build -t vse:1.0.36 .
```

如果要使用別的 Dockerfile 來製作 Image，可以用以下指令 (以 `cuda.dockerfile` 這個 dockerfile 當作範例)：
```
docker build -t vse:1.0.36 -f cuda.dockerfile .
```

在方案資料夾開啟終端機，然後輸入以下指令將已經製作好的 Docker Image 匯出成 tar 檔並放到方案資料夾中：
```
docker save vse:1.0.36 -o vse_1_0_36.tar
```

其他執行 Docker 的伺服器在拿到打包好的 tar 檔後，可以使用以下指令將 tar 檔重新載入回 Image：
```
docker load --input vse_1_0_36.tar
```

### 從 Image 建立 Container

完成 Image 的建立後，如果要建立 Container 請使用 run 指令，並且在指令加上參數 `--gpus all` 來將 GPU 資源分配給該 Container。
您也可以直接用 Docker Desktop 來建立 Container，但是這樣該 Container 就不會支援 GPU 加速功能。
在建立 Container 時，您會需要指定一個 port 做為 VSE 系統對外的服務 port；除了 port 外，還要指定一個 Volumn 空間 (路徑為 `/app/Storage`) 讓 VSE 系統存放資料庫等檔案，此 Volume 空間可在多次建立 Container 時沿用。
下列是範例指令 (指定 port 8082 為服務 port，指定本機資料夾 `C:\Volume\vse` 對應到 Container 內的 `/app/Storage` 資料夾)：
```
docker run --gpus all --name vse --env=PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin 
--env=ASPNETCORE_URLS=http://+:80 --env=DOTNET_RUNNING_IN_CONTAINER=true --env=DOTNET_VERSION=6.0.21 
--env=ASPNET_VERSION=6.0.21 --volume=C:\Volume\vse:/app/Storage 
--workdir=/app -p 8082:80 --runtime=runc -d vse:1.0.36
```

## 建立支援 GPU 加速的測試環境

為了要能測試製作出來的 Image，會需要建立一套能支援 GPU 加速的測試環境。
目前 VSE 系統會用到 GPU 加速的環節只有將影片切成串流的工作，這部分是使用 ffmpeg 工具進行。
如果您希望直接使用 Visual Studio 對專案進行 Debug，那 Windows 版的 ffmpeg 已經預設支援，您只需確認電腦已經安裝了 Nvidia 10 代後的顯示卡，並且安裝了支援 CUDA 的驅動程式。

### 0. 專業版 Windows 11 或 Windows Server
如果非 Windows Server，就一定要是 Windows 11，Windows 10 不支援 GPU 轉 Docker。

### 1. 啟用 WSL 支援
使用指令啟用 WSL
```
dism.exe /online /enable-feature /featurename:Microsoft-Windows-Subsystem-Linux /all /norestart
```

使用指令啟用虛擬化
```
dism.exe /online /enable-feature /featurename:VirtualMachinePlatform /all /norestart
```

### 2. 安裝和設定 WSL

使用指令來安裝 WSL (參考文件：https://learn.microsoft.com/zh-tw/windows/wsl/install)
```
wsl --install
```

完成安裝後，可以使用指令 `wsl -l -v` 來列出虛擬機，預設 WSL 可能會安裝一台 Ubuntu。
接著您需要先將 WSL 的預設版本設定為 `2`，使用指令：
```
wsl --set-default-version 2
```

完成後，要來裝這次要用來當作測試環境的虛擬機。開啟 Windows Store 然後搜尋 "Ubuntu"，找到想要的版本做安裝 (建議找最新版)。
完成安裝後，在 Windows Store 內點選該機器的"開啟"按鈕，完成使用者設定後虛擬機就裝好了。這台虛擬機會出現在新安裝的 APP 清單中，建議設個捷徑方便未來使用。
如果再下一次 `wsl -l -v` 指令，應可看到新裝好的虛擬機。

### 3. 安裝 Docker Desktop
去官網下載然後直接裝起來 (如果已裝好就跳過)，記得要選用 WSL 模式。

裝好後，進到 Settings > Resources > WSL Integration 畫面，找到前一個步驟裝好的 Ubuntu 機器並啟動他。

### 4. 安裝 CUDA Toolkit
在新裝的 Ubuntu 上清除舊的 nvidia 驅動程式，如果有舊的驅動程式遺留，那後續步驟將會失敗。指令：
```
sudo apt purge nvidia*
sudo apt autoremove
sudo apt autoclean
rm -rf /usr/local/cuda*
```

到 CUDA Toolkit 網站 (https://developer.nvidia.com/cuda-toolkit-archive) 找到最新的版本。
點進去最新版後需要設定要下載的版本，選 Linux > x86_64 > WSL-Ubuntu > 2.0 > deb (network) 後，複製下方出現的指令，按照指令在新裝的 Ubuntu 上執行。如果 network 版因為遺漏檔案裝失敗，也可以改用 local 版來安裝 (大概要下載 20GB 的檔案)。

完成所有指令的執行後請重新開機。重開後使用指令測試驅動程式：`sudo nvidia-smi`，然後使用指令來測試 CUDA：`/usr/local/cuda/bin/nvcc --version`。

### 5. 安裝 nvidia container toolkit
參考官方文件：
https://docs.nvidia.com/datacenter/cloud-native/container-toolkit/latest/install-guide.html

### 6. 使用 Nvidia 官方的 Docker image 來驗證硬體加速

使用指令將 Ubuntu 設定為預設的虛擬機：`wsl --setdefault 名稱`。

開啟 Docker Desktop，然後搜尋 nvidia/cuda，選擇對應 Ubuntu 版本的 Image 後，將該 Image Pull 下來。Image 有分成 base, runtime 和 devel，選擇 base 或 runtime 就行了，但 Ubunte 版本一定要對。

開啟 PowerShell，使用指令 run 來從 Image 建立 Container (下面指令的版本請自行修改) 指令：
```
docker run --gpus all --name nvidia/cuda -it -d nvidia/cuda:12.2.0-runtime-ubuntu22.04
```

在 Docker Desktop 應可見到新的 Container 被建立，在 Terminal 內檢查是否有錯誤。

驗證完成後記得關掉 Container，以後建立 Container 只要給予 `--gpus all` 參數，就可以支援 GPU 運算。

### 7. 編譯 ffmpeg 讓 ffmpeg 可以支援 GPU 運算 (非必要)。
這個步驟僅針對想要拿到能支援 GPU 的 ffmpeg 時才要執行。
準備一台已經走完步驟 5 的 Ubuntu 機器。
使用指令 vim ~/.bashrc 來編輯 .bashrc 檔，這裡要加入 nvcc 的路徑到 PATH 內。加上三行：
```
export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/usr/local/cuda/lib64
export CUDA_HOME=/usr/local/cuda
export PATH=$PATH:/usr/local/cuda/bin
```
完成後要重開。

接著參考流程：
https://docs.nvidia.com/video-technologies/video-codec-sdk/12.0/ffmpeg-with-nvidia-gpu/index.html

上面流程只會建立最最基本包含 cuda encoder/decoder 的版本，如果想要建立可支援其他格式的 ffmpeg 就會需要在 Configure 時給予特定參數。
這個專案已經將 cuda 和常用 coder 打包成一個 Script：
https://github.com/markus-perl/ffmpeg-build-script

編譯好的 ffmpeg 要放在 `/src/SubtitleEditor.Web/FFMpeg` 內，通常沒有附檔名。編譯好後除了 ffmpeg 外，連 ffprobe 也要複製過去。

使用 cuda.dockerfile 來製作 Image，就會使用編譯過的 ffmpeg。

## 代辦事項

### 支援 GPU 的 ffmpeg 惡夢

直接用 apt install ffmpeg 裝好的版本不支援 GPU，所以無論如何都一定要自己編譯才行。
直接使用 ffmpeg-build-script 專案的 script 編譯並給予參數 `--build --enable-gpl-and-non-free` 後，大概跑一個小時能夠產出支援 GPU 的版本。
但這樣產出的 ffmpeg 沒辦法搬到別的地方使用，搬出去就會碰到 library 路徑找不到的問題，因為此 ffmpeg 編譯相依於 CUDA Toolkit。
這導致此方式編譯出的 ffmpeg 沒辦法用在 docker image 內。

第二種作法，是使用 script 編譯時額外給予參數 `--full-static`，這樣就可以解決 library 的相依性。
但很可惜地，將 ffmpeg 搬到 docker image 內以後，雖然一般的指令都可以運作，碰到最關鍵的 `-hwaccel cuda` 時卻會出現 `dl-call-libc-early-init.c:37: _dl_call_libc_early_init: Assertion 'sym != NULL' failed` 這樣的錯誤訊息。
這個錯誤應是 glibc 造成的，但實際修正的方法於網路上沒有任何資料，會需要理解 C 語言，然後找出編譯時出問題的句子在哪並予以修正。

為了要能夠讓 ffmpeg 支援 gpu，最後一種方法就是直接在 dockerfile 內寫入安裝 CUDA Toolkit、nvidia container toolkit 的指令，然後直接在 image 產生過程安裝相關套件並編譯 ffmpeg。
微軟官方提供的 Asp.net core docker image 不可能做到這件事，只能以 Nvidia 提供的 CUDA runtime docker image 為基礎，編譯好 ffmpeg 後再重做一次 Asp.net core runtime 環境。
這樣製作出來的 image 會有 20GB 大，但應該可以用才對，這個方法還沒有試過。
