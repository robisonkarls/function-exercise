import { Logo } from "./Logo";
import { Button } from "./Button";

export default function Header() {
  function handleSendOffer() {
    const to = "robison.karls@gmail.com";
    const subject = encodeURIComponent("Welcome to the Function Health Team");
    const body = encodeURIComponent("Welcome to the Function Health Team");
    window.location.href = `mailto:${to}?subject=${subject}&body=${body}`;
  }

  return <header className="flex items-center justify-between">
    <Logo/>
    <div className="flex flex-col gap-1">
      <h1 className="text-3xl font-bold text-zinc-700">Todo List</h1>
    </div>
    
    <div className="flex flex-col gap-1 items-end">
      <Button className="bg-blue-500 text-white hover:bg-blue-700" onClick={handleSendOffer}>Send offer to Robison</Button>
    </div>
  </header>
}