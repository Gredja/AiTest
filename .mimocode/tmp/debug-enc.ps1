$path = 'D:\Work\AI\curriculum-public\Gredja\Ui\AssemblyInfo.cs'
$reader = [System.IO.StreamReader]::new($path)
$text = $reader.ReadToEnd()
$encoding = $reader.Encoding
$reader.Dispose()
Write-Host "enc-null: $($null -eq $encoding)"
Write-Host "enc: $encoding"
