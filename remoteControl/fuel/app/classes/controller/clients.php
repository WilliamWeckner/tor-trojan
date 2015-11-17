<?php

class Controller_Clients extends Controller_Template
{
    public function __construct()
    {
        if(!Session::get('user'))
            Response::redirect('/');
    }

    public function action_index()
    {
        $this->template->content = View::forge('clients/index');
    }
}