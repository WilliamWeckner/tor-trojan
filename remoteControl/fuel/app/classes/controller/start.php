<?php

class Controller_Start extends Controller
{
	public function action_index()
	{
		return new Response(null, 404);
	}
}