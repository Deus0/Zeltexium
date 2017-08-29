<?php
  /* Create a folder */
    if(is_dir($_REQUEST['DirectoryName']) === false)
    {
      mkdir($_REQUEST['DirectoryName']);
    }
  /* Upload Image data! */
     $folder = $_REQUEST['FileName']; //"../Public/".
     if (is_uploaded_file($_FILES['Data']['tmp_name']))
     {
         if (move_uploaded_file($_FILES['Data']['tmp_name'], $folder)) 
         {
             Echo 1;//'File uploaded';
         }
     }
     else
     {
         Echo 0;//'File is not uploaded.';
     }
?>