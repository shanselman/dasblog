function Tidy-Xml {
    begin {
        $private:str = ""
        
        # recursively concatenate strings from passed-in arrays of schmutz
        # not sure how to improve this...
        function ConcatString ([object[]] $szArray) {
            # return string
            $private:rStr = ""

            # Recursively call itself, if a string is also of array or a collection type
            foreach ($private:sz in $szArray) {
                if (($private:sz.GetType().IsArray) -or `
                    ($private:sz -is [System.Collections.IList])) {
                    $private:rStr += ConcatString($private:sz)
                }
                elseif ($private:sz -is [xml]) {
                    $private:rStr += $private:sz.Get_OuterXml()
                }
                else {
                    $private:rStr += $private:sz
                }
            }
            return $private:rStr;
        }
        
        # Original "Tidy-Xml" portion
        function FormatXmlString ($arg) {
            # ignore parse errors
            trap { continue; }
            
            # out-null hides output of the assembly load
            [System.Reflection.Assembly]::LoadWithPartialName("System.Xml") | out-null

            $PRIVATE:tempString = ""
            if ($arg -is [xml]){
                $PRIVATE:tempString = $arg.get_outerXml()
            }
            if ($arg -is [string]){
                $PRIVATE:tempString = $arg
            }

            # the ` tick mark is a line-continuation char
            $r = new-object System.Xml.XmlTextReader(`
                new-object System.IO.StringReader($PRIVATE:tempString))
            $sw = new-object System.IO.StringWriter
            $w = new-object System.Xml.XmlTextWriter($sw)
            $w.Formatting = [System.Xml.Formatting]::Indented

            do { $w.WriteNode($r, $false) } while ($r.Read())

            $w.Close()
            $r.Close()
            $sw.ToString()
        }
    }
    
    process {
        # For non-xml strings or types, they will be buffered and will be
        # taken care of in "end" block
        
        # this checks for objects that have been "pipe'd" in.
        if ($_) {
            # check if whatever we have appended is a valid XML or not
            $private:xmlStr = ($private:str + $_) -as [xml]
            
            if ($private:xmlStr -ne $null) {
                FormatXmlString([xml]$private:xmlStr)
                # clear the string not to be handled in "end" block
                $private:str = $null
            } else {
                if ($_ -is [string]) {
                    $private:str += $_
                } elseif ($_ -is [xml]) {
                    FormatXmlString($_)
                }
                # for an array or a collection type,
                elseif ($_.Count) {
                    # iterate each item in the collection and append
                    foreach ($i in $_) {
                        $private:line += $i
                    }
                    $private:str += $private:line
                }
            }
        }
    }

    end {
        if ([string]::IsNullOrEmpty($private:str)) {
            $private:szXml = $(ConcatString($args)) -as [xml]
            if (! [string]::IsNullOrEmpty($private:szXml)) {
                FormatXmlString([xml]$private:szXml)
            }
        } else {
            FormatXmlString([xml]$private:str)
        }
    }
}
