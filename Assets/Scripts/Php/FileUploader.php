<?php
  /* Create a folder */
  if(is_dir($_REQUEST['DirectoryName']) == false)
  {
    mkdir($_REQUEST['DirectoryName']);
  }
  /* Upload file data! */
  $MyData = $_REQUEST['Data'];
  $MyFile = fopen($_REQUEST['FileName'], "w");
  fwrite($MyFile, $MyData);
  fclose($MyFile);
?>
