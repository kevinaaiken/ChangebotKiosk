using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Project_Changebot{
    class Program{

        static Random rnd = new Random();

        static void Main(string[] args){
            //INITALIZE
            int transaction_num = 0;
            //LOG: 0 = transaction #, 1 = date, 2 = time, 3 = cash amount, 4 = card vendor, 5 = card amount, 6 = change given
            string[] log_package = new string[7];
            Dictionary<decimal, int> dic_currency = new Dictionary<decimal, int> ();
            #region Amount of Currency
            int Hundreds =  10;
            int Fifties =   10;
            int Twenties =  10;
            int Tens =      10;
            int Fives =     10;
            int Twos =      10;
            int OnesB =     10;
            int OnesC =     10;
            int FiftyCent = 10;
            int Quarters =  10;
            int Dimes =     10;
            int Nickles =   10;
            int Pennies =   10;
            #endregion
            #region Populate Dictionary
            dic_currency.Add(100.00m, Hundreds);
            dic_currency.Add(50.00m, Fifties);
            dic_currency.Add(20.00m, Twenties);
            dic_currency.Add(10.00m, Tens);
            dic_currency.Add(5.00m, Fives);
            dic_currency.Add(2.00m, Twos);
            dic_currency.Add(1.00m, OnesB);
            dic_currency.Add(0.999m, OnesC);
            dic_currency.Add(0.50m, FiftyCent);
            dic_currency.Add(0.25m, Quarters);
            dic_currency.Add(0.10m, Dimes);
            dic_currency.Add(0.05m, Nickles);
            dic_currency.Add(0.01m, Pennies);
            #endregion
            bool operating = Initalize_Till(Hundreds, Fifties, Twenties, Tens, Fives, Twos, OnesB, 
                                            OnesC, FiftyCent, Quarters, Dimes, Nickles, Pennies);
            
            //KIOSK OPERATING LOOP
            while (operating){
                //prepare to enter items
                for (int i = 0; i < log_package.Length; i ++){
                    log_package[i] = "";
                }//reset log package for next transaction
                transaction_num ++;
                int item_counter =   0;
                decimal item_total = 0.00m;
                string item_price =  "0.00";

                //ENTER ITEMS LOOP
                while (item_price != ""){
                    item_counter ++;
                    Console.Write("Item {0}\t\t", item_counter);
                    item_price = Console.ReadLine();
                    if (item_price != ""){
                        item_total = item_total + Convert.ToDecimal(item_price);
                    }//end if(change)
                }//while(entering items)
                //prepare to pay
                Display_Total(item_total);
                decimal original_total = item_total;
                decimal change_togive = 0;
                decimal cash_back = 0;
                int attempt_counter = 0;
                int selection = 0;
                bool verify_change = false;
                
                //PAYMENT LOOP
                while (item_total > 0.01m){
                    verify_change = false;
                    attempt_counter ++; //add when failed transaction
                    //transaction failed
                    if (attempt_counter > 4 || selection == 3){
                        Console.WriteLine("TRANSACTION CANCELLED");
                        log_package[3] = "";  //cash payment = 0
                        log_package[5] = "";  //card payment = 0
                        if ((original_total - item_total) > 0.01m){
                            change_togive = original_total - item_total;
                        }//if(partially paid then cancel: return money)
                        item_total = 0;
                    }else{
                        //select payment type
                        selection = Convert.ToInt32(Prompt("Select Payment Type:\n1 = Cash\n2 = Credit/Debit\n3 = Cancel\n\nOption: "));
                        while (selection < 1 || selection > 3){  //input validation
                            selection = Convert.ToInt16(Prompt("Please select 1 or 2: "));
                        }//end while(bad selection)
                        Console.WriteLine();  //formatting
                        
                        //CASH PAYMENT
                        if (selection == 1){  //cash
                            change_togive = Cash_Payment(dic_currency, item_total);
                            verify_change = Change_Dispense(dic_currency, change_togive, verify_change);
                            if (verify_change == false){
                                Console.Write("NOT ENOUGH CHANGE TO COMPLETE TRANSACTION");
                                change_togive = original_total - item_total;
                                selection = 3;
                            }//if(not enough change)
                            Console.WriteLine();  //formatting
                            //cash not paid in full
                            if (change_togive < 0){ 
                                //if change is returned as (-) then only partially paid in cash
                                log_package[3] = (item_total + change_togive).ToString("F");
                                item_total = change_togive * -1;
                                change_togive = 0.0m;
                                Display_Total(item_total);
                                attempt_counter --;
                            //cash paid in full
                            }else{
                                if (log_package[4] == ""){
                                    log_package[3] = original_total.ToString("F");  //partially paid in cash, then paid in cash again
                                    item_total = 0;
                                }else{
                                    log_package[3] = item_total.ToString("F");  //partially paid with card, then finished paying in cash
                                    item_total = 0;
                                }//end if(didn't use card)
                            }//end if(not paid in full)
                        
                        //PREPARE CREDIT CARD PAYMENT
                        }else if (selection == 2){  //card
                            string cc_number = Prompt("Enter credit card number: ");
                            Console.WriteLine("Processing...");  //formatting
                            Console.WriteLine(CreditCard_Reader(cc_number, log_package));
                            bool cc_valid = Luhn(cc_number);
                            //cashback option
                            if (cc_valid){
                                int selection2 = Convert.ToInt16(Prompt("Cashback?\n1 = Yes\n2 = No\n\nOption: "));
                                while (selection2 < 1 || selection2 > 2){  //input validation
                                    selection2 = Convert.ToInt16(Prompt("Please select 1 or 2: "));
                                }//end while(bad selection2)
                                if (selection2 == 1){
                                    //yes cashback
                                    cash_back = Convert.ToDecimal(Prompt("How much cashback: "));
                                    while (cash_back < 0 || cash_back > 500.1m){  //input validation
                                        cash_back = Convert.ToDecimal(Prompt("Try a different amount: "));
                                    }//end while(bad selection2)
                                    verify_change = Change_Dispense(dic_currency, cash_back, verify_change);
                                    if (verify_change == false){
                                        Console.WriteLine("INSUFFICENT FUNDS FOR CASHBACK ");
                                        cash_back = 0;
                                    }//if(not enough change)
                                    item_total = item_total + cash_back;
                                    change_togive = change_togive + cash_back;
                                //no cashback
                                }else{
                                }//end if(selection2)

                                //PAY WITH CARD
                                string[] request_answer = MoneyRequest(cc_number, item_total);
                                //card declined
                                if (request_answer[1] == "declined"){
                                    item_total = item_total - cash_back;
                                    cash_back = 0;
                                    change_togive = 0;
                                    Console.WriteLine(request_answer[1].ToUpper());
                                    Display_Total(item_total);
                                //card accepted
                                }else{
                                    //paid in full
                                    if ((item_total - Convert.ToDecimal(request_answer[1])) < 0.01m){
                                        Display_Total(item_total);
                                        log_package[5] = item_total.ToString("F");
                                        item_total = item_total - Convert.ToDecimal(request_answer[1]);
                                        Console.WriteLine("PAYMENT ACCEPTED REMOVE CARD");
                                    //insufficent funds
                                    }else if (item_total > 0){
                                        Console.WriteLine("INSUFFICENT FUNDS");
                                        item_total = item_total - cash_back;
                                        cash_back = 0;
                                        change_togive = 0;
                                        Display_Total(item_total);
                                    }//end if(card paid in full)
                                }//end if(pay with card)
                            }else{
                                Console.WriteLine("INVALID CARD NUMBER");
                            }//end if(valid)
                        }//end if(selection)
                    }//end if(too many attempts)
                }//while(total > 0)

                //CHANGE
                if (change_togive < 0.01m){
                }else{
                    Change_Dispense(dic_currency, change_togive, verify_change);
                    log_package[6] = change_togive.ToString("F");
                }//end if(change)
                Console.WriteLine("\nThank you! Come again. Press ANY key to end transaction.");
                log_package[0] = transaction_num.ToString().PadLeft(4, '0');//transaction #
                log_package[1] = DateTime.Now.ToString("MM'-'dd'-'yyyy");
                log_package[2] = DateTime.Now.ToString("HH:mm:ss");//get time
                if (log_package[3] == ""){
                    log_package[3] = "0.00";
                }//end if(no cash)
                if (log_package[4] == ""){
                    log_package[4] = "na";
                }//end if(no vendor)
                if (log_package[5] == ""){
                    log_package[5] = "0.00";
                }//end if(no card)
                if (log_package[6] == ""){
                    log_package[6] = "0.00";
                }//end if(no change)
                string log_file = (log_package[0] + " " +log_package[1] + " " + log_package[2] + " $" + log_package[3] + " " +log_package[4] + " $" + log_package[5] + " $" + log_package[6]);
                //0 = transaction #, 1 = date, 2 = time, 3 = cash amount, 4 = card vendor, 5 = card amount, 6 = change given
                Transaction_Logger(log_file);  //transaction logging package
                Console.ReadKey(true);
                Console.Clear();
            }//end while(kiosk operating)
        }//END MAIN

        static string Prompt (string message){
            Console.Write(message);
            return Console.ReadLine();
        }//end prompt function

        static bool Initalize_Till (int Hundreds, int Fifties, int Twenties, int Tens, int Fives, int Twos, int OnesB, 
                                    int OnesC, int FiftyCent, int Quarters, int Dimes, int Nickles, int Pennies){
            //hardware check to show all coin slots are sending a signal
            return true;
        }//end initialize till function

        static void Transaction_Logger (string x){
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "C:\\Users\\CCA030\\source\\kaa\\Changebot_Logger\\Changebot_Logger\\bin\\Debug\\Changebot_Logger.exe";
            startInfo.Arguments = x;
            Process.Start(startInfo);
        }//end transaction logger
            
        static void Display_Total (decimal x){
            Console.WriteLine("Total\t{0}", x.ToString("F"));
            Console.WriteLine();  //formatting
        }//end display total function

        static string CreditCard_Reader (string CardNum, string[] x){
            if(CardNum[0] == '3'){
                x[4] = "American_Express";
                return("American Express " + CardNum);
            }else if(CardNum[0] == '4'){
                x[4] = "Visa_Debit/Credit";
                return("Visa Debit/Credit " + CardNum);
            }else if(CardNum[0] == '5'){
                 x[4] = "MasterCard_Debit/Credit";
                return("MasterCard Debit/Credit "  + CardNum);
            }else if(CardNum[0] == '6'){
                x[4] = "Discover_Debit/Credit";
                return("Discover Debit/Credit " + CardNum);
            }else{
                return("That card number was not found in our database");
            }//end if(card type ok)
        }//end credit reader

        static bool Luhn(string digits){
            return digits.All(char.IsDigit) && digits.Reverse()
                .Select(c => c - 48)
                .Select((thisNum, i) => i % 2 == 0
                    ? thisNum
                    : ((thisNum *= 2) > 9 ? thisNum - 9 : thisNum)
                ).Sum() % 10 == 0;
        }//end Luhn algorithm function

        static string[] MoneyRequest (string account_number, decimal amount){
            Random rnd = new Random();
            //50% CHANCE TRANSACTION PASSES OR FAILS
            bool pass = rnd.Next(100) <50;
            //50% CHANCE THAT A FAILED TRANSACTION IS DECLINED
            bool declined = rnd.Next(100) < 50;
            if (pass){
                return new string[] {account_number, amount.ToString()};
            }else{
                if(!declined){
                    return new string[] {account_number, (amount / rnd.Next(2, 6)).ToString()};
                }else{
                    return new string[] {account_number, "declined"};
                }//end if
            }//end if
        }//end money request function
        
        static decimal Cash_Payment (Dictionary<decimal, int> x, decimal total_toreceive){
            //PREPARE TO RECEIVE PAYMENT
            int payment_counter = 0;
            int temp_definition;
            decimal change = 0;
            //continue cash allows you to pay partially with cash and rest on card
            bool continue_cash = true;
            //send dictionary keys to array
            decimal[] ary_bills = x.Keys.ToArray();
            //RECEIVE PAYMENT
            while (total_toreceive > 0 && continue_cash){
                payment_counter ++;
                bool proper_input = false;
                Console.Write("Payment {0}\t", payment_counter);
                //user attempts to enter bill
                string bill_orcoinS = Console.ReadLine();
                //cash continue or stop
                if (bill_orcoinS.ToString() == ""){
                    continue_cash = false;
                }else{
                    //validate bill
                    decimal bill_orcoin = Convert.ToDecimal(bill_orcoinS);
                    foreach (decimal denomination in ary_bills){
                        if (bill_orcoin == denomination){
                            proper_input = true;
                        }//if(can accept bill or coin)
                    }//end foreach(currency)
                    if (proper_input){
                        //update bill count
                        temp_definition = x[bill_orcoin];
                        temp_definition = temp_definition + 1;
                        x[bill_orcoin] = temp_definition;
                        //subtract from total
                        if (bill_orcoin == 0.999m){
                            total_toreceive = total_toreceive - 1.00m;
                        }else{
                            total_toreceive = total_toreceive - bill_orcoin;
                        }//end if(dollar coin)
                        //display remaining or change
                         if (total_toreceive > 0){
                            Console.WriteLine("Remaining\t{0}", total_toreceive.ToString("F"));
                        }else if (total_toreceive < 0){
                            decimal change_positive = total_toreceive * -1;
                            Console.WriteLine("\nChange\t{0}", change_positive.ToString("F"));
                        }//end if(remaining, change or finished)
                    }else{
                        Console.WriteLine("Enter valid bill or coin");
                        payment_counter --;
                    }//end if(proper input)
                }//end if(finished paying with cash)
            }//while(total >= 0)
            //convert total to change
            change = total_toreceive * -1;
            return change;
        }//end item total function

        static bool Change_Dispense (Dictionary<decimal, int> x, decimal change_togive, bool enough_change){
            decimal temp_change = change_togive;
            if (enough_change){
                Greedy_Algorithm (x, change_togive, enough_change);
                return true;
            }else{
                enough_change = Greedy_Algorithm (x, temp_change, enough_change);
                return enough_change;
            }//end if(have change to give)
        }//end change dispense function
                
        static bool Greedy_Algorithm (Dictionary<decimal, int> x, decimal change, bool check_change){
            #region Get Bill Count
            int hundreds =  x[100.00m];
            int fifties =   x[50.00m];
            int twenties =  x[20.00m];
            int tens =      x[10.00m]; 
            int fives =     x[5.00m];
            int twos =      x[2.00m];
            int onesB =     x[1.00m];
            int onesC =     x[0.999m];
            int fiftycent = x[0.50m];
            int quarters =  x[0.25m];
            int dimes =     x[0.10m];
            int nickles =   x[0.05m];
            int pennies =   x[0.01m];
            #endregion
            if (change >= 100.00m){
                while (change >= 100.00m && hundreds > 0){
                    hundreds = hundreds - 1;
                    if (check_change){
                        Console.WriteLine("$100.00 bill dispensed");
                        x[100.00m] = hundreds;
                    }//end if(checked)
                    change = change - 100.00m;
                }//end while(dispense hundreds)
            }//end if(>hundred)
            if (change >= 50.00m){
                while (change >= 50.00m && fifties > 0){
                    fifties = fifties - 1;
                    if (check_change){
                        Console.WriteLine("$50.00 bill dispensed");
                        x[50.00m] = fifties;
                    }//end if(checked)
                    change = change - 50.00m;
                }//end while(dispense fifties)
            }//end if(>fifty)
            if (change >= 20.00m){
                while (change >= 20.00m && twenties > 0){
                    twenties = twenties - 1;
                    if (check_change){
                        Console.WriteLine("$20.00 bill dispensed");
                        x[20.00m] = twenties;
                    }//end if(checked)
                    change = change - 20.00m;
                }//end while(dispense twenties)
            }//end if(>twenty)
            if (change >= 10.00m){
                while (change >= 10.00m && tens > 0){
                    tens = tens - 1;
                    if (check_change){
                        Console.WriteLine("$10.00 bill dispensed");
                        x[10.00m] = tens;
                    }//end if(checked)
                    change = change - 10.00m;
                }//end while(dispense tens)
            }//end if(>ten)
            if (change >= 5.00m){
                while (change >= 5.00m && fives > 0){
                    fives = fives - 1;
                    if (check_change){
                        Console.WriteLine("$5.00 bill dispensed");
                        x[5.00m] = fives;
                    }//end if(checked)
                    change = change - 5.00m;
                }//end while(dispense fives)
            }//end if(>five)
            if (change >= 2.00m){
                while (change >= 2.00m && twos > 0){
                    twos = twos - 1;
                    if (check_change){
                        Console.WriteLine("$2.00 bill dispensed");
                        x[2.00m] = twos;
                    }//end if(checked)
                    change = change - 2.00m;
                }//end while(dispense twos)
            }//if(>two)
            if (change >= 1.00m){
                while (change >= 1.00m && onesB > 0){
                    onesB = onesB - 1;
                    if (check_change){
                        Console.WriteLine("$1.00 bill dispensed");
                        x[1.00m] = onesB;
                    }//end if(checked)
                    change = change - 1.00m;
                }//end while(dispense onesB)
            }//if(>oneB)
            if (change >= 1.00m){
                while (change >= 1.00m && onesC > 0){
                    onesC = onesC - 1;
                    if (check_change){
                        Console.WriteLine("$1.00 coin dispensed");
                        x[0.999m] = onesC;
                    }//end if(checked)
                    change = change - 1.00m;
                }//end while(dispense onesC)
            }//if(>oneC)
            if (change >= 0.50m){
                while (change >= 0.50m && fiftycent > 0){
                    fiftycent  = fiftycent  - 1;
                    if (check_change){
                         Console.WriteLine("$0.50 coin dispensed");
                        x[0.50m] = fiftycent;
                    }//end if(checked)
                    change = change - 0.50m;
                }//end while(dispense fiftycent)
            }//end if(>fiftycent)
            if (change >= 0.25m){
                while (change >= 0.25m && quarters > 0){
                    quarters = quarters - 1;
                    if (check_change){
                        Console.WriteLine("$0.25 coin dispensed");
                        x[0.25m] = quarters;
                    }//end if(checked)
                    change = change - 0.25m;
                }//end while(dispense quarters)
            }//end if(>quarter)
            if (change >= 0.10m){
                while (change >= 0.10m && dimes > 0){
                    dimes  = dimes  - 1;
                    if (check_change){
                        Console.WriteLine("$0.10 coin dispensed");
                        x[0.10m] = dimes;
                    }//end if(checked)
                    change = change - 0.10m;
                }//end while(dispense quarters)
            }//end if(>dime)
            if (change >= 0.05m){
                while (change >= 0.05m && nickles > 0){
                    nickles = nickles - 1;
                    if (check_change){
                        Console.WriteLine("$0.05 coin dispensed");
                        x[0.05m] = nickles;
                    }//end if(checked)
                    change = change - 0.05m;
                }//end while(dispense nickles)
            }//end if(>nickle)
            if (change >= 0.01m){
                while (change >= 0.01m && pennies > 0){
                    pennies = pennies - 1;
                    if (check_change){
                        Console.WriteLine("$0.01 coin dispensed");
                        x[0.01m] = pennies;
                    }//end if(checked)
                    change = change - 0.01m;
                }//end while(dispense pennies)
            }//end if(have currency)
            if (change == 0 || change < 0.01m){
                return true;
            }else{
                return false;
            }//end if change checked
        }//end greedy algorithm function

    }//class
}//name