<?php

class Controller_Panel extends Controller_Template
{
    public function action_index()
    {
        $user = Session::get('user');
        if($user)
        {
            $this->template->content = View::forge('panel/news');
        }
        else
            $this->template->content = View::forge('panel/index');
    }

    public function action_logout()
    {
        Session::delete('user');
        Response::redirect('panel');
    }

    public function post_index()
    {
        $username = Input::post('username', null);
        $password = Input::post('password', null);

        try
        {
            if($username == 'admin' && $password == 'admin')
                Session::set('user', 1);
        }
        catch(\Exception $e)
        { }

        Response::redirect('panel');
    }
}
