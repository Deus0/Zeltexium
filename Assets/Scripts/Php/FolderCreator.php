<?php
  /* Create a folder */
if(is_dir($_REQUEST['FileName']) === false)
{
  mkdir($_REQUEST['FileName']);
}
?>
