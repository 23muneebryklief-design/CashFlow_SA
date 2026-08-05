import Nav from "../../components/layout/Nav/Nav";
import Hero from "../../components/Hero/Hero";
import Ticker from "../../components/Ticker/Ticker";
import HowItWorks from "../../components/HowItWorks/HowItWorks";
import FeaturedDeals from "../../components/FeaturedDeals/FeaturedDeals";
import Security from "../../components/security/Security";
import Footer from "../../components/layout/Footer/Footer";

export default function Home() {
  return (
    <>
      <Nav />
      <Hero />
      <Ticker />
      <HowItWorks />
      <FeaturedDeals />
      <Security />
      <Footer />
    </>
  );
}