<?php
  /* Create a folder */
  $MyDirectory = $_REQUEST['DirectoryName'];
  if(is_dir($MyDirectory) != false)
  {
    $MyFiles = scandir($MyDirectory);
    foreach ($MyFiles as $MyFile) 
    {
      if ($MyFile != "." && $MyFile != "..") 
      {
        echo "$MyFile\n";
      }
    }
  } 
  else 
  {
    echo "Failed to find: $MyDirectory";
  }
?>
