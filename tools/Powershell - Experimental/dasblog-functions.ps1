filter Remove-Category 
{
	param(
		[string]$category=$(throw '$category is required')
		)
	foreach($file in $_)
	{
		$a = [xml](get-content $file)
		write-progress -Activity "Scanning..." -Status "$file" -CurrentOperation " "
		
		trap { "Error in $file!" }
		#set an Author if there isn't one (possibly from older imported posts)
		$dirty = $false
		$a.DayEntry.Entries.Entry | foreach-object { 
				if( ($_.Categories -match $category) -eq $true ) {
							write-progress -Activity "Scanning..." -Status "$file" -CurrentOperation "Removing Category $category..."
							#only remove the category if it has other categories
							if ($_.Categories.Length -ne $category.Length) {
								$temp = ""
								($_.Categories.Split(";")) | where { $_ -match $category -eq $false } | foreach-object { $temp += $_ + ";" }
								$_.Categories = $temp.TrimEnd(";");
							}
							"Removed $category from $file..."
							$_.Categories
							$dirty = $true
						}
					}
		}
		if ($dirty) {
			$a | tidy-xml | set-content ($file.Name)
		}
}



filter Tag-DasBlog 
{
	param(
		[string]$category=$(throw '$category is required'), 
		[string]$word=$category
		)
	foreach($file in $_)
	{
		$a = [xml](get-content $file)
		write-progress -Activity "Scanning..." -Status "$file" -CurrentOperation " "
		
		trap { "Error in $file!" }
		#set an Author if there isn't one (possibly from older imported posts)
		$dirty = $false
		$a.DayEntry.Entries.Entry | foreach-object { 
				if( ($_.Categories -match $category) -eq $false ) {
					if ($_.Content -imatch $word) {
							write-progress -Activity "Scanning..." -Status "$file" -CurrentOperation "Adding Category $category..."
							#if it's not already there
							if ($_.Categories -imatch $category -eq $false)
							{
								if ($_.Categories.Length -gt 0) {
									$_.Categories += ";" 
								}
								$_.Categories += $category
								"Added $category to $file..."
								$_.Categories
								$dirty = $true
							}
						}
					}
		}
		if ($dirty) {
			$a | tidy-xml | set-content ($file.Name)
		}
	}
}

filter Tidy-DasBlog 
{
	$author = "admin"
	$namespace = "urn:newtelligence-com:dasblog:runtime:data"
	
	foreach($file in $_)
	{
		$a = [xml](get-content $file)
		write-progress -Activity "Scanning..." -Status "$file" -CurrentOperation " "
		
		trap { "Error in $file!" }
		#set an Author if there isn't one (possibly from older imported posts)
		$dirty = $false
		$a.DayEntry.Entries.Entry | foreach-object { 
		
			if ($_.Content.StartsWith("<p>",[System.StringComparison]::CurrentCultureIgnoreCase) -eq $false) { 
				$_.Content = "<p>" + $_.Content
				$dirty = $true
				"Adding <p> to content in $file..."
			}
			if ($_.Content.EndsWith("</p>",[System.StringComparison]::CurrentCultureIgnoreCase) -eq $false) { 
				$_.Content = $_.Content += "</p>"
				$dirty = $true
				"Adding </p> to content in $file..."
			}

			if ($_.Author -eq $null) { 
					write-progress -Activity "Scanning..." -Status "$file" -CurrentOperation "Adding Author..."

					$b = $a.CreateElement("Author",$namespace);
					$b.set_InnerText($author) | out-null
					$_.AppendChild($b) | out-null
					$dirty = $true
					"Adding Author to content in $file..."
					} 
		
			#Create an empty Categories node if there isn't one. We'll need it later.
			if ($_.Categories -eq $null) { 
					write-progress -Activity "Scanning..." -Status "$file" -CurrentOperation "Adding Category Node..."
					$b = $a.CreateElement("Categories",$namespace);
					$_.AppendChild($b) | out-null 
					$dirty = $true
					"Adding Categories to content in $file..."
					} 
		}
		if ($dirty) {
			$a | tidy-xml | set-content ($file.Name)
		}
	}
}
