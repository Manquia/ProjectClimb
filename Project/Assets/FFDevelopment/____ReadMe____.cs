/*
 * Fail Faster Development Unity Package 
 * Ver: 0.3
 * 
 * Thank you for downloading the FFDevelopment Unity Package. This
 * package contains a few useful systems/components/classes which I
 * feel are paramount to rapid development in Unity3d.
 *  
 * Contains:
 * 
 * Example Files (*EX) files
 * This project includes some simple files for how to use each part
 * of the system in a basic way. This should give you a quick start
 * on some of the things you can do with this tool package.
 * 
 * FFAction:
 * This is a robust action system which can be used to setup logic
 * which happens over time, instantly, in parralell, or in sequence.
 * This is useful for a creating a lot of different behavior but
 * might initially be considered for stage-based AI, Controller
 * behavior, Enemy Behaviors, and general alteration of data over
 * time.
 * 
 * FFMessage/FFMessageBox/FFMessageBoard:
 * -FFMessage is a global message system which is easy to connect,
 * disconnect, and send. It is simple, flexable, and performanant
 * for any global events.
 * -FFMessageBox is basically identical with
 * the main difference being that it is a class which can be added
 * to a component for local events/messaging.
 * -FFMessageBoard is a entry-based eventsystem with gameobject
 * locallity implimented. This allows events to be sent to object
 * which then specific scripts may listen to.
 * 
 * FFPath:
 * FFPath is a basic path component which has no build-in controller
 * and a simple interface. It can be used to create DynamicPaths
 * which can alter the number of points and their position
 * while in-game.
 * 
 * FFComponent:
 * FFComponent is a wrapper for Monobehavior and is useful
 * because it adds a getter for FFAction and adds FFPosition and
 * FFScale to pass to FFAction for changing the position of
 * an object over time.
 *  
 * FFMeta:
 * This contains FFVar<> and FFRef<> which are used to create
 * what some other languages call a Property Delegate which is
 * then used by FFAction. These types may be re-used whenever
 * useful for other system, but I would recomend a full
 * investigation of the class before-hand. To do most of your
 * work with FFAction you shouldn't need to use this because
 * of FFComponent's wrapper on transform.position/scale
 * 
 * FFVector:
 * A few data fields to allow for serialized types of Unity types.
 * 
 * 
 * FFMessageSystem:
 * The Central system which all FFMessage/FFMessageBoards use to
 * connect to the net. This is can also be used to tracks stats
 * of every event passed in your game.
 * 
 * FFSystem:
 * Holds a bunch of data and functionaity used by other systems
 * including time and netids. It is also responsible for disbatching
 * messages from the net.
 * 
 * 
 * * ------------------------------------------------------------------
 * 
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
 * GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 * ------------------------------------------------------------------
 * 
 * ******************************************************************
 * |                            Credits                             |
 * |                                                                |  
 * |                                                                |
 * |                           Creator                              |
 * |                          Micah Rust                            |
 * |                                                                |
 * |                           Testers                              |
 * |                      Victoria Dominowski                       |
 * |                       Gabriel Oropeza                          |
 * |                      Kaylin Norman-Slack                       |
 * ******************************************************************
 * 
 * 
 * Further Notes: You may notice there are some hooks for networking in 
 * the messaging system. I haven't finished a good demo of how to use 
 * that specific system but it isn't that hard to figure out. Feel free
 * to add your own networking code if you want/need the functionality.
 */

