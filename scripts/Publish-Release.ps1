[CmdletBinding(SupportsShouldProcess)]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string] $Version,

    [string] $Branch = 'main'
)

$ErrorActionPreference = 'Stop'

if ($Version.StartsWith('v')) {
    $Version = $Version.Substring(1)
}

if ($Version -notmatch '^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$') {
    throw "版本号必须符合 SemVer，例如 1.0.1 或 1.0.1-rc.1。"
}

$tag = "v$Version"

function Invoke-Git {
    param([Parameter(Mandatory = $true)][string[]] $Arguments)
    & git @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "git $($Arguments -join ' ') 执行失败，退出码 $LASTEXITCODE。"
    }
}

$root = (& git rev-parse --show-toplevel).Trim()
if ($LASTEXITCODE -ne 0) {
    throw '当前目录不在 Git 仓库中。'
}
Set-Location $root

$status = & git status --porcelain
if ($LASTEXITCODE -ne 0) {
    throw '无法读取 Git 工作区状态。'
}
if ($status) {
    throw "工作区不干净，请先提交或处理以下变更：`n$status"
}

$currentBranch = (& git branch --show-current).Trim()
if ($currentBranch -ne $Branch) {
    throw "当前分支为 '$currentBranch'，发布必须从 '$Branch' 分支执行。"
}

$remote = (& git remote get-url origin).Trim()
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($remote)) {
    throw '未配置 origin 远端。'
}

Write-Host "检查 origin/$Branch 和远端标签..."
Invoke-Git @('fetch', 'origin', $Branch, '--tags', '--quiet')

$existingTag = & git ls-remote --tags origin "refs/tags/$tag"
if ($LASTEXITCODE -ne 0) {
    throw '无法检查远端标签。'
}
if ($existingTag) {
    throw "远端标签 $tag 已存在，不会覆盖已有发布。"
}
if (git tag --list $tag) {
    throw "本地标签 $tag 已存在，请删除或换一个版本号。"
}

$head = (& git rev-parse HEAD).Trim()
$remoteHead = (& git rev-parse "origin/$Branch").Trim()
if ($head -ne $remoteHead) {
    if ($PSCmdlet.ShouldProcess("origin/$Branch", '推送当前提交')) {
        Invoke-Git @('push', 'origin', "HEAD:$Branch")
    }
}

if (-not $PSCmdlet.ShouldProcess($tag, '创建并推送发布标签')) {
    return
}

Invoke-Git @('tag', '-a', $tag, '-m', "Release $tag")
try {
    Invoke-Git @('push', 'origin', $tag)
}
catch {
    & git tag -d $tag | Out-Null
    throw
}

Write-Host "已推送 $tag。GitHub Actions 将构建并发布 Sirius.Protocol 与 Sirius.MasterData 包。" -ForegroundColor Green
